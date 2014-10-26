@rem cnEq.cmd := (path-to)\cnEq.py %*
@
call cnEq 0.txt 0.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123a.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123b.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123c.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123d.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123e.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123f.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123-cr.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123-crlf.txt
@echo errorlevel=%errorlevel%
call cnEq -1.txt 123-lf.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123-cr.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123-crlf.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123a.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123b.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123c.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123d.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123e.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123f.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt 123-crlf.txtd
@echo errorlevel=%errorlevel%
call cnEq vvv123.txt 123-crlf.txt
@echo errorlevel=%errorlevel%
@rem bad
call cnEq -1.txt 123g.txt
@echo errorlevel=%errorlevel%
call cnEq 123.txt ../bad/123-bad-1.txt
@echo errorlevel=%errorlevel%
call cnEq ../bad/123-bad-1.txt 123.txt
@echo errorlevel=%errorlevel%
