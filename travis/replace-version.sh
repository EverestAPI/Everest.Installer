#!/bin/bash
if TAG=`git describe --exact-match --tags 2>/dev/null`; then
  VERSION=${TAG#"v"}
else
  VERSION="0.0.0.${TRAVIS_BUILD_NUMBER}"
fi
perl -0777 -pi -e 's/\[assembly: AssemblyVersion\(".*"\)\]/\[assembly: AssemblyVersion\("'${VERSION}'"\)\]/gm' ./Properties/AssemblyInfo.cs
