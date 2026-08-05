# Embedded re-linq core

This directory contains the re-linq core source from version 2.1.2, upstream
commit `88b1055e0a737faff26c9d5e2789f520ac73ca86`.

Upstream project: https://github.com/re-motion/Relinq

The source is licensed under Apache License 2.0; see `LICENSE.txt`.

Local changes:

- namespaces were moved from `Remotion.Linq` to `Akzin.Crm.Linq.Relinq`;
- shared utility and annotation namespaces were moved below that namespace;
- the .NET 4.5 reflection compatibility shims are omitted because Mono and
  .NET Framework 4.5.2 provide those APIs themselves;
- the source is compiled directly into `Akzin.Crm.Linq.dll`.
