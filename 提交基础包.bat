@echo off
cd %~dp0
git subtree split --prefix=Assets/_Basic --branch ump
git tag 1.0.1 ump
git push origin ump --tags
pause
