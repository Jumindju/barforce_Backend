docker run -it --rm -v %cd%\config:/config -v %cd%\reports:/reports crossbario/autobahn-testsuite wstest -m fuzzingclient -s /config/fuzzingclient.json
pause