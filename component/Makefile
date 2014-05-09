MONOXBUILD=/Library/Frameworks/Mono.framework/Commands/xbuild
MDTOOL=/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool

all: compileAll

android: compileAndroid

ios: compileiOS

compileAndroid:
	$(MONOXBUILD) /p:Configuration=Release ../Rivets/Rivets.Android.csproj
	
compileiOS:
	$(MDTOOL) -v build -t:Build "-c:Release|iPhone" ../Rivets/Rivets.iOS.csproj
	
compileAll: compileAndroid compileiOS

clean:
	-rm -rf lib/monotouch/ lib/monoandroid/ lib/wp8/
