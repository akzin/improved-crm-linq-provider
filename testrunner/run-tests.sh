#!/bin/sh
set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
project_dir=$(dirname "$script_dir")
configuration=${1:-Debug}
runner="$script_dir/mstest-lite.exe"
test_output="$project_dir/Akzin.Crm.Linq.Tests/bin/$configuration"
library_output="$project_dir/Akzin.Crm.Linq/bin/$configuration"

case "$configuration" in
    Debug|Release) ;;
    *)
        echo "Configuration must be Debug or Release" >&2
        exit 2
        ;;
esac

cd "$project_dir"
xbuild Akzin.Crm.Linq.sln /t:Rebuild /p:Configuration="$configuration" /verbosity:minimal
mcs -out:"$runner" "$script_dir/MsTestLite.cs"

MONO_PATH="$test_output:$library_output:$project_dir/packages/MSTest.TestFramework.1.2.0/lib/net45:$project_dir/packages/Moq.4.7.145/lib/net45:$project_dir/packages/Castle.Core.4.2.1/lib/net45" \
    mono "$runner" "$test_output/Akzin.Crm.Linq.Tests.dll"
