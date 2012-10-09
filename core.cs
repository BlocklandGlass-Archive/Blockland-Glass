new ScriptGroup(BLG) {
	internalVersion = "2.0.DEV";
	externalVersion = "2.0 Development";
	versionId = 1337; //1 is anything before 1.2

	debugLevel = 3;
	//0 = Errors only
	//1 = Standard
	//2 = In-depth
	//3 = Spam me. Please.
	required = false;

	sound = "Add-Ons/System_BlocklandGlass/sound";
};

function BLG::start(%this, %implementation) {
	%this.implementation = %implementation;

	if(isFile("config/BLG/prefs.cs")) {
		exec("config/BLG/prefs.cs");
	}


	exec("./Support_Byte.cs");
	exec("./script/support/encrypt.cs");
	exec("./script/support/queue.cs");
	
	if(%implementation $= "server") {
		echo("Loading BLG [" @ %this.internalVersion @ "] server implementation");

		exec("./script/support/serverConnection.cs");

		exec("./script/server/bindManager.cs");
		exec("./script/server/guiDownloader.cs");
		exec("./script/server/hudManager.cs");
		exec("./script/server/imageDownloader.cs");
		exec("./script/server/remoteControl.cs");

		if(isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
			exec("./script/server/hooks/RTB.cs");
		} else {
			exec("./script/server/hooks/default.cs");
		}

	} else if(%implementation $= "client") {
		if(!$BLG::Pref::InstallerHasRun) {
			echo("Loading BLG installer");

			exec("./installer/main.cs");
			return 0;
		} else {
			echo("Loading BLG [" @ %this.internalVersion @ "] client implementation");

			exec("./script/support/serverConnection.cs");
			exec("./script/support/encrypt.cs");
			exec("./script/support/animation.cs");

			exec("./gui/profile.cs");
			exec("./gui/BLG_HUD.gui");
			exec("./gui/BLG_remapGui.gui");
			exec("./gui/BLG_keybindGui.gui");
			exec("./gui/BLG_Updater.gui");
			exec("./gui/BLG_Home.gui");
			exec("./gui/BLG_RemoteControl.gui");
			exec("./gui/BLG_SelectServer.gui");
			exec("./gui/BLG_Overlay.gui");

			exec("./script/client/bindManager.cs");
			exec("./script/client/guiDownloader.cs");
			exec("./script/client/hudManager.cs");
			exec("./script/client/notification.cs");
			exec("./script/client/openOverlay.cs");
			exec("./script/client/imageDownloader.cs");
			exec("./script/client/remoteControl.cs");
			exec("./script/client/updater.cs");

			if(isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
				exec("./script/client/hooks/RTB.cs");
			} else {
				exec("./script/client/hooks/default.cs");
			}

			if($BlockOS::Enabled) {
				exec("./script/client/hooks/BOS.cs");
			}
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
		return 0;
	}
	return 1;
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

function BLG::error(%this, %msg) {
	%this.debug(%msg, 0);
}

function BLG::setRequired(%this) {
	%this.required = true;
}

function BLG::getOverlay(%this) {
	if(isObject(RTB_Overlay)) {
		return RTB_Overlay;
	} else {
		return BLG_Overlay;
	}
}

package BLG_Package {
	function onExit() {
		echo("Exporting BLG Prefs");
		export("$BLG::*", "config/BLG/prefs.cs");
		parent::onExit();
	}
};
activatePackage(BLG_Package);
