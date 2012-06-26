new ScriptGroup(BLG) {
	internalVersion = "2.0 Dev";
	externalVersion = "2.0 Development";
	versionId = 1337; //1 is anything before 1.2

	debugLevel = 3;
	//0 = Errors only
	//1 = Standard
	//2 = In-depth
	//3 = Spam me. Please.
	required = false;
};

function BLG::start(%this, %implementation) {
	%this.implementation = %implementation;

	exec("./script/support/serverConnection.cs");
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
		exec("./gui/BLG_Desktop.gui");
		exec("./gui/BLG_HUD.gui");
		exec("./gui/BLG_remapGui.gui");
		exec("./gui/BLG_keybindGui.gui");
		exec("./gui/BLG_Updater.gui");
		exec("./gui/BLG_Home.gui");
		exec("./gui/BLG_RemoteControl.gui");
		exec("./gui/BLG_Overlay.gui");

		exec("./script/client/bindManager.cs");
		exec("./script/client/desktop.cs");
		exec("./script/client/guiDownloader.cs");
		exec("./script/client/hudManager.cs");
		exec("./script/client/openOverlay.cs");
		exec("./script/client/imageDownloader.cs");
		exec("./script/client/updater.cs");

		if(isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
			exec("./script/client/hooks/RTB.cs");
		} else {
			exec("./script/client/hooks/default.cs");
		}

		if($BlockOS::Enabled) {
			exec("./script/client/hooks/BOS.cs");
		}

		if(!isFile("config/BLG/firstrun")) {
			%this.firstRun = true;
			%fo = new FileObject();
			%fo.openforwrite("config/BLG/firstrun");
			%fo.writeline("Jimmy ran first.");
			%fo.close();
			%fo.delete();
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