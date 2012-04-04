new ScriptGroup(BLG) {
	internalVersion = "1.0.A1";
	externalVersion = "1.0 Alpha 1";

	implementation = ""; //server or client
	loaded = false;

	debugLevel = 1;
	//0 = No Debug
	//1 = Standard
	//2 = In-depth
	//3 = Spam me. Please.
};

function BLG::initiatorLoaded(%this) {
	if(%this.implementation $= "server") {
		echo("Loading BLG [" @ %this.internalVersion @ "] server implementation");
		exec("./script/server/guiServer.cs");
	} else if(%this.implementation $= "client") {
		echo("Loading BLG [" @ %this.internalVersion @ "] client implementation");
		exec("./script/client/guiClient.cs");
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

	echo("\c5BLG Debug >>\c1 " @ %msg);
}