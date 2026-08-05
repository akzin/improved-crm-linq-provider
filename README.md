*Akzin.Crm.Linq* is an improved Linq provider for Microsoft CRM, with many additional features over the Microsoft CRM SDK's Linq Provider.

Documentation and news: <https://akzin.com/>. Install the package from
[NuGet](https://www.nuget.org/packages/Akzin.Crm.Linq):

```
PM> Install-Package Akzin.Crm.Linq
```

Quick Start
===========

```
using Akzin.Crm.Linq;

IOrganizationService crm = CreateCrmContext();

var q = from c in crm.Query<Contact>()
    join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
    where acc.Name == "Akzin.com"
    select new { a = acc, c };

var listOfAccountsAndContacts = q.ToList();
```

Supported features (infinite list)
========

- Early-bound and late-bound support
- Outer left joins (SDK Linq goes only one level deep)
- Count
- Paging
- Returns more than 5000 records
- ...

Examples
=======

Filter on Enum equivalents of `OptionSetValue`
```
from a in crm.Query<Account>()
where a.StatusCodeEnum == AccountStatuscode.Active
select a;
```

Return Enum values in anonymous types
```
from a in crm.Query<Account>()
select new { a.Name, a.StatusCodeEnum };
```

Join two tables and return the second one
```
from c in crm.Query<Contact>()
join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
select acc;
```

Use Any
```
var q = from c in crm.Query<Contact>()
    where a.StatusCodeEnum == AccountStatuscode.Active
    select acc;

var any = q.Any();
```

...

Feature Request and Contribution
===============

Feature Requests should be posted on GitHub (or email: to@fik.email). When requesting a feature, try to provide example code.

You could also checkout the code and implement the feature yourself. Your contribution using pull requests, is much appreciated.

License
=======

The *Akzin.Crm.Linq* project is licensed under the *Apache Software License 2.0*.
Copyright 2018 and onwards Akzin.com.

The full license text is in [LICENSE](LICENSE). The attributions and third-party
notices are in [NOTICE](NOTICE).

This project contains work from [Relinq](https://github.com/re-motion/Relinq),
also under the Apache 2.0 license; its license text is kept in
`Akzin.Crm.Linq/ThirdParty/Relinq/LICENSE.txt`.
