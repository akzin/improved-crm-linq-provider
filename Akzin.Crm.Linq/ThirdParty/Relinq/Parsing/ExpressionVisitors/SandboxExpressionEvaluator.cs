using System;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Akzin.Crm.Linq.Relinq.Parsing.ExpressionVisitors
{
  /// <summary>
  /// Evaluates parameter-free expression trees without LambdaExpression.Compile.
  /// Dynamics CRM sandbox code must not depend on runtime assembly/IL generation.
  /// </summary>
  internal static class SandboxExpressionEvaluator
  {
    public static object Evaluate (Expression expression)
    {
      if (expression == null)
        return null;

      switch (expression.NodeType)
      {
        case ExpressionType.Constant:
          return ((ConstantExpression) expression).Value;
        case ExpressionType.MemberAccess:
          return EvaluateMember ((MemberExpression) expression);
        case ExpressionType.Call:
          return EvaluateCall ((MethodCallExpression) expression);
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
        case ExpressionType.TypeAs:
          return EvaluateConvert ((UnaryExpression) expression);
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.Not:
          return EvaluateUnary ((UnaryExpression) expression);
        case ExpressionType.New:
          return EvaluateNew ((NewExpression) expression);
        case ExpressionType.NewArrayInit:
        case ExpressionType.NewArrayBounds:
          return EvaluateNewArray ((NewArrayExpression) expression);
        case ExpressionType.MemberInit:
          return EvaluateMemberInit ((MemberInitExpression) expression);
        case ExpressionType.ListInit:
          return EvaluateListInit ((ListInitExpression) expression);
        case ExpressionType.Conditional:
          return EvaluateConditional ((ConditionalExpression) expression);
        case ExpressionType.Invoke:
          return EvaluateInvocation ((InvocationExpression) expression);
        case ExpressionType.TypeIs:
          return ((TypeBinaryExpression) expression).TypeOperand.IsInstanceOfType (Evaluate (((TypeBinaryExpression) expression).Expression));
        case ExpressionType.Default:
          return expression.Type.IsValueType ? Activator.CreateInstance (expression.Type) : null;
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
        case ExpressionType.Divide:
        case ExpressionType.Modulo:
        case ExpressionType.And:
        case ExpressionType.AndAlso:
        case ExpressionType.Or:
        case ExpressionType.OrElse:
        case ExpressionType.ExclusiveOr:
        case ExpressionType.Equal:
        case ExpressionType.NotEqual:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
        case ExpressionType.LessThan:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.Coalesce:
        case ExpressionType.ArrayIndex:
          return EvaluateBinary ((BinaryExpression) expression);
        default:
          throw new NotSupportedException ("Sandbox evaluation does not support expression type: " + expression.NodeType);
      }
    }

    private static object EvaluateMember (MemberExpression expression)
    {
      var instance = Evaluate (expression.Expression);
      var field = expression.Member as FieldInfo;
      if (field != null)
        return field.GetValue (instance);

      var property = expression.Member as PropertyInfo;
      if (property != null)
        return Invoke (property.GetGetMethod (true), instance, new object[0]);

      throw new NotSupportedException ("Unsupported member: " + expression.Member);
    }

    private static object EvaluateCall (MethodCallExpression expression)
    {
      return Invoke (expression.Method, Evaluate (expression.Object), EvaluateArguments (expression.Arguments));
    }

    private static object EvaluateConvert (UnaryExpression expression)
    {
      var value = Evaluate (expression.Operand);
      if (expression.NodeType == ExpressionType.TypeAs)
        return value == null || expression.Type.IsInstanceOfType (value) ? value : null;
      if (value == null)
        return null;

      var targetType = Nullable.GetUnderlyingType (expression.Type) ?? expression.Type;
      if (targetType.IsInstanceOfType (value))
        return value;
      if (targetType.IsEnum)
        return Enum.ToObject (targetType, value);
      return Convert.ChangeType (value, targetType, CultureInfo.InvariantCulture);
    }

    private static object EvaluateUnary (UnaryExpression expression)
    {
      if (expression.Method != null)
        return Invoke (expression.Method, null, new[] { Evaluate (expression.Operand) });

      var value = Evaluate (expression.Operand);
      if (expression.NodeType == ExpressionType.Not && value is bool)
        return !(bool) value;
      if (expression.NodeType == ExpressionType.Not)
        return ConvertIntegral (~Convert.ToInt64 (value, CultureInfo.InvariantCulture), expression.Type);
      return ConvertNumber (-Convert.ToDecimal (value, CultureInfo.InvariantCulture), expression.Type);
    }

    private static object EvaluateNew (NewExpression expression)
    {
      if (expression.Constructor == null)
        return Activator.CreateInstance (expression.Type);
      return InvokeConstructor (expression.Constructor, EvaluateArguments (expression.Arguments));
    }

    private static object EvaluateNewArray (NewArrayExpression expression)
    {
      var elementType = expression.Type.GetElementType ();
      if (expression.NodeType == ExpressionType.NewArrayBounds)
      {
        var lengths = EvaluateArguments (expression.Expressions);
        var dimensions = new int[lengths.Length];
        for (var i = 0; i < lengths.Length; i++)
          dimensions[i] = Convert.ToInt32 (lengths[i], CultureInfo.InvariantCulture);
        return Array.CreateInstance (elementType, dimensions);
      }

      var array = Array.CreateInstance (elementType, expression.Expressions.Count);
      for (var i = 0; i < expression.Expressions.Count; i++)
        array.SetValue (Evaluate (expression.Expressions[i]), i);
      return array;
    }

    private static object EvaluateMemberInit (MemberInitExpression expression)
    {
      var instance = EvaluateNew (expression.NewExpression);
      foreach (var binding in expression.Bindings)
      {
        var assignment = binding as MemberAssignment;
        if (assignment == null)
          throw new NotSupportedException ("Only member assignments are supported in sandbox evaluation.");
        SetMember (assignment.Member, instance, Evaluate (assignment.Expression));
      }
      return instance;
    }

    private static object EvaluateListInit (ListInitExpression expression)
    {
      var instance = EvaluateNew (expression.NewExpression);
      foreach (var initializer in expression.Initializers)
        Invoke (initializer.AddMethod, instance, EvaluateArguments (initializer.Arguments));
      return instance;
    }

    private static object EvaluateConditional (ConditionalExpression expression)
    {
      return (bool) Evaluate (expression.Test) ? Evaluate (expression.IfTrue) : Evaluate (expression.IfFalse);
    }

    private static object EvaluateInvocation (InvocationExpression expression)
    {
      var target = (Delegate) Evaluate (expression.Expression);
      return target.DynamicInvoke (EvaluateArguments (expression.Arguments));
    }

    private static object EvaluateBinary (BinaryExpression expression)
    {
      if (expression.NodeType == ExpressionType.AndAlso)
      {
        var leftBoolean = (bool) Evaluate (expression.Left);
        return leftBoolean && (bool) Evaluate (expression.Right);
      }
      if (expression.NodeType == ExpressionType.OrElse)
      {
        var leftBoolean = (bool) Evaluate (expression.Left);
        return leftBoolean || (bool) Evaluate (expression.Right);
      }

      var left = Evaluate (expression.Left);
      if (expression.NodeType == ExpressionType.Coalesce)
        return left ?? Evaluate (expression.Right);
      var right = Evaluate (expression.Right);

      if (expression.Method != null)
        return Invoke (expression.Method, null, new[] { left, right });
      if (expression.NodeType == ExpressionType.ArrayIndex)
        return ((Array) left).GetValue (Convert.ToInt32 (right, CultureInfo.InvariantCulture));
      if (expression.NodeType == ExpressionType.Equal)
        return Equals (left, right);
      if (expression.NodeType == ExpressionType.NotEqual)
        return !Equals (left, right);

      var comparable = left as IComparable;
      if (expression.NodeType == ExpressionType.GreaterThan)
        return comparable.CompareTo (right) > 0;
      if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
        return comparable.CompareTo (right) >= 0;
      if (expression.NodeType == ExpressionType.LessThan)
        return comparable.CompareTo (right) < 0;
      if (expression.NodeType == ExpressionType.LessThanOrEqual)
        return comparable.CompareTo (right) <= 0;

      if (expression.NodeType == ExpressionType.And || expression.NodeType == ExpressionType.Or || expression.NodeType == ExpressionType.ExclusiveOr)
      {
        var leftValue = Convert.ToInt64 (left, CultureInfo.InvariantCulture);
        var rightValue = Convert.ToInt64 (right, CultureInfo.InvariantCulture);
        var result = expression.NodeType == ExpressionType.And ? leftValue & rightValue :
            expression.NodeType == ExpressionType.Or ? leftValue | rightValue : leftValue ^ rightValue;
        return ConvertIntegral (result, expression.Type);
      }

      var leftNumber = Convert.ToDecimal (left, CultureInfo.InvariantCulture);
      var rightNumber = Convert.ToDecimal (right, CultureInfo.InvariantCulture);
      decimal number;
      switch (expression.NodeType)
      {
        case ExpressionType.Add:
        case ExpressionType.AddChecked: number = leftNumber + rightNumber; break;
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked: number = leftNumber - rightNumber; break;
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked: number = leftNumber * rightNumber; break;
        case ExpressionType.Divide: number = leftNumber / rightNumber; break;
        case ExpressionType.Modulo: number = leftNumber % rightNumber; break;
        default: throw new NotSupportedException ("Unsupported binary expression: " + expression.NodeType);
      }
      return ConvertNumber (number, expression.Type);
    }

    private static object[] EvaluateArguments (System.Collections.ObjectModel.ReadOnlyCollection<Expression> expressions)
    {
      var arguments = new object[expressions.Count];
      for (var i = 0; i < expressions.Count; i++)
        arguments[i] = Evaluate (expressions[i]);
      return arguments;
    }

    private static object Invoke (MethodInfo method, object instance, object[] arguments)
    {
      try { return method.Invoke (instance, arguments); }
      catch (TargetInvocationException exception) { throw exception.InnerException; }
    }

    private static object InvokeConstructor (ConstructorInfo constructor, object[] arguments)
    {
      try { return constructor.Invoke (arguments); }
      catch (TargetInvocationException exception) { throw exception.InnerException; }
    }

    private static void SetMember (MemberInfo member, object instance, object value)
    {
      var field = member as FieldInfo;
      if (field != null) { field.SetValue (instance, value); return; }
      var property = member as PropertyInfo;
      if (property != null) { property.SetValue (instance, value, null); return; }
      throw new NotSupportedException ("Unsupported initialized member: " + member);
    }

    private static object ConvertIntegral (long value, Type type)
    {
      return Convert.ChangeType (value, Nullable.GetUnderlyingType (type) ?? type, CultureInfo.InvariantCulture);
    }

    private static object ConvertNumber (decimal value, Type type)
    {
      return Convert.ChangeType (value, Nullable.GetUnderlyingType (type) ?? type, CultureInfo.InvariantCulture);
    }
  }
}
