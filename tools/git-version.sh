#! /bin/sh

assembly=Assembly/AssemblyInfo

full_version=`../tools/git-version-gen --prefix v .tarball-version`
version=`echo $full_version | sed -e 's/-/\t/' | cut -f 1 | sed -e 's/UNKNOWN/0.0.0/'` 

sed -e "s/@FULL_VERSION@/$full_version/" -e "s/@VERSION@/$version/" $assembly.in > $assembly.cs-
cmp -s $assembly.cs $assembly.cs- || mv $assembly.cs- $assembly.cs
rm -f $assembly.cs-

sed -e "s/@FULL_VERSION@/$full_version/" -e "s/@VERSION@/$version/" ModularStorageContainer.dox.in > ModularStorageContainer.dox-
cmp -s ModularStorageContainer.dox ModularStorageContainer.dox- || mv ModularStorageContainer.dox- ModularStorageContainer.dox
rm -f ModularStorageContainer.dox-
