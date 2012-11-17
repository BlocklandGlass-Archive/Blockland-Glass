if(!isObject(BLG_GAU)) {
	new ScriptObject(BLG_GAU) {
		host = "api.blockland.zivle.com";
	};
	BLG_GAU.tcp = new TCPObject(BLG_GAU_TCP);
	BLG_GAU_TCP.parent = BLG_GAU;
}

function BLG_GAU_TCP::getVersion(%this, %manual) {
	%manual += 0;
	%this.manual = %manual;
	%this.update = false;
	%this.connect(%this.parent.host @ ":80");
}

function BLG_GAU_TCP::onConnected(%this) {
	%str = "mod=update&n=" @ $Pref::Player::NetName @ "&arg1=VERSION&arg2=" @ BLG.internalVersion;

	%post = "POST / HTTP/1.1";
	%post = %post @ "\nHost: " @ %this.parent.host;
	%post = %post @ "\nUser-Agent: BLG/" @ BLG.internalVersion;
	%post = %post @ "\nContent-Length:" SPC strlen(%str);
	%post = %post @ "\nContent-Type: application/x-www-form-urlencoded";
	%post = %post @ "\n\n" @ %str @ "\n";

	%this.send(%post);
}

function BLG_GAU_TCP::onLine(%this, %line) {
	BLG.debug(%line, 3);
	if(getField(%line, 0) $= "UPDATE") {
		%ver = getField(%line, 1);
		BLG_GAU.version = %ver;
		BLG_SUS.newUpdate("Blockland Glass", %ver, "BLG_GAU.confirmUpdate");
	} else if(getField(%line, 0) $= "END") {
		%this.disconnect();
	}
}

function BLG_GAU_TCP::onDisconnect(%this) {
	if(!%this.update) {
		if(%this.manual) {
			messageBoxOk("No update", "There are currently no updates available.");
		}
	}
}

function BLG_GAU::downloadVersion(%this, %version) {
	%tcp = new TCPObject(BLG_GAU_Downloader) {
		version = %version;
	};

	%tcp.connect("api.blockland.zivle.com:80");
}

function BLG_GAU::confirmUpdate(%this) {
	%this.downloadVersion(%this.version);
}

function BLG_GAU_Downloader::onConnected(%this) {
	%data = "n=" @ $Pref::Player::NetName @ "&version=" @ %this.version @ "&cur=" @ BLG.internalVersion;

	%tString = "POST /update.php HTTP/1.1\n";
	%tString = %tString @ "Host: api.blockland.zivle.com\n";
	%tString = %tString @ "Content-Type: application/x-www-form-urlencoded\n";
	%tString = %tString @ "Content-Length: " @ strLen(%data) @ "\n";
	%tString = %tString @ "\n" @ %data @ "\n";

	%this.send(%tString);
}

function BLG_GAU_Downloader::onLine(%this, %line) {
	BLG.debug(%line);
	if(strPos(%line, "Content-Length:") >= 0)
		%this.size = getWord(%line, 1);
	
	if(%line $= "NOTFOUND")
		BLG.error("File not found!");

	if(%line $= "Content-Type: application/zip") {
		%this.isFile = true;
	}

	if(%line $= "" && %this.isFile) {
		%this.setBinarySize(%this.size);
	}
}

function BLG_GAU_Downloader::onBinChunk(%this, %chunk) {
	if(%this.startTime $= "")
		%this.startTime = getSimTime();

	if(%chunk < %this.size)
	{
		echo("BLG Download: " @ mFloor((%chunk/%this.size)*100) @ "%");
	}
	else
	{
		if(isWriteableFilename("Add-Ons/System_BlocklandGlass.zip") && isWriteableFilename("config/BLG/updater/temp.zip"))
		{
			%this.saveBufferToFile("config/BLG/updater/temp.zip"); //Just incase we crash for some reason, we dont leave some sort of corrupted shit
			
			fileCopy("config/BLG/updater/temp.zip", "Add-Ons/System_BlocklandGlass.zip");
			fileDelete("config/BLG/updater/temp.zip");
		}
		else
			BLG.error("READ ONLY");

	}
}

package BLG_Updater_Package {
	function postServerTCPObj::connect(%this, %addr) {
		parent::connect(%this, %addr);
		if($Server::Dedicated && !BLG_GAU_TCP.checked) {
			echo("Checking BLG update");
			BLG_GAU_TCP.checked = true;
			BLG_GAU_TCP.getVersion();
		}
	}
};
activatePackage(BLG_Updater_Package);