new ScriptGroup(BLG) {
	internalVersion = "1.3.1";
	externalVersion = "1.3.1";
	versionId = 3; //1 is anything before 1.2

	debugLevel = 0;
	//0 = Errors only
	//1 = Standard
	//2 = In-depth
	//3 = Spam me. Please.
	required = false;
};

function BLG::start(%this, %implementation) {
	%this.implementation = %implementation;

	if(%implementation $= "server") {
		echo("Loading BLG [" @ %this.internalVersion @ "] server implementation");

		exec("./script/server/bindManager.cs");
		exec("./script/server/guiDownloader.cs");
		exec("./script/server/hudManager.cs");
		exec("./script/server/imageDownloader.cs");

		if(isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
			exec("./script/server/hooks/RTB.cs");
		} else {
			exec("./script/server/hooks/default.cs");
		}

	} else if(%implementation $= "client") {
		echo("Loading BLG [" @ %this.internalVersion @ "] client implementation");

		exec("./gui/profile.cs");
		exec("./gui/BLG_HUD.gui");
		exec("./gui/BLG_remapGui.gui");
		exec("./gui/BLG_keybindGui.gui");
		exec("./gui/BLG_Updater.gui");

		exec("./script/client/bindManager.cs");
		exec("./script/client/guiDownloader.cs");
		exec("./script/client/hudManager.cs");
		exec("./script/client/imageDownloader.cs");
		exec("./script/client/updater.cs");

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
	} else {
		echo("\c5BLG Debug >>\c1 " @ %msg);
	}
}

function BLG::setRequired(%this) {
	%this.required = true;
}