docker run -it --rm -v %cd%\config:/config -v %cd%\reports:/reports -p 9001:9001 crossbario/autobahn-testsuite wstest -m fuzzingserver -s /config/fuzzingserver.json
pause