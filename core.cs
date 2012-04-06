new ScriptGroup(BLG) {
	internalVersion = "1.0.A1";
	externalVersion = "1.0 Alpha 1";

	debugLevel = 1;
	//0 = Errors only
	//1 = Standard
	//2 = In-depth
	//3 = Spam me. Please.
};

function BLG::start(%this, %implementation) {
	%this.implementation = %implementation;

	if(%implementation $= "server") {
		echo("Loading BLG [" @ %this.internalVersion @ "] server implementation");
		exec("./script/server/guiDownloader.cs");

		if(isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
			exec("./script/server/hooks/RTB.cs");
		} else {
			exec("./script/server/hooks/default.cs");
		}

	} else if(%implementation $= "client") {
		echo("Loading BLG [" @ %this.internalVersion @ "] client implementation");
		exec("./script/client/guiDownloader.cs");

		if(isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
			exec("./script/client/hooks/RTB.cs");
		} else {
			exec("./script/client/hooks/default.cs");
		}

	} else {
		%this.debug("Unresolved Initiator");
		error("Failed to load BLG [" @ %this.internalVersion @ "]. Please redownload from http://blocklandglass.com");
	} 
}

function BLG::debug(%this, %msg, %level) {
	if(%level $= "") {
		%level = 1;
	}

	if(%level > %this.debugLevel) {
		return;
	}
	if(%level == 0) {
		echo("\c2BLG Error >> " @ %msg);
	}
	echo("\c5BLG Debug >>\c1 " @ %msg);
}