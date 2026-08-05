# Test runner

Run the complete test suite from the project root with:

```sh
./testrunner/run-tests.sh
```

Use `./testrunner/run-tests.sh Release` to build and test the Release configuration.
The script builds the solution with Mono `xbuild`, compiles the lightweight MSTest
runner with `mcs`, and executes every `[TestMethod]` in a `[TestClass]`.

This runner supports the MSTest features currently used by this project, including
`[ExpectedException]`. It is intentionally not a complete MSTest implementation.
