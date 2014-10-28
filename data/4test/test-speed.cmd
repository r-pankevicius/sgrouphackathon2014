GOTO 16K

@echo *** Indianapolis 1950 version ***
@time /t
REM ..\..\src\mortals\sharptools\cnEqIndy500.py 512M\a 512M\a
@time /t

@echo 

@echo *** Melbourne 2013 version ***
@time /t
..\..\src\mortals\sharptools\cnEq.py 512M\a 512M\a
@time /t


:16K

@echo *** Indianapolis 1950 version ***
@time /t
REM ..\..\src\mortals\sharptools\cnEqIndy500.py 16K\-a 16K\-a
@time /t

@echo 

@echo *** Melbourne 2013 version ***
@time /t
..\..\src\mortals\sharptools\cnEq.py 16K\-a 16K\-a
@time /t
