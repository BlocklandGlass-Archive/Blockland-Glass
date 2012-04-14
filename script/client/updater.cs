if(!isObject(BLG_GAU)) {
	new ScriptObject(BLG_GAU) {
		host = "api.blockland.zivle.com";
	};
	BLG_GAU.tcp = new TCPObject(BLG_GAU_TCP);
	BLG_GAU_TCP.parent = BLG_GAU;
}

function BLG_GAU_TCP::getVersion(%this) {
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
	echo(%line);
	if(getField(%line, 0) $= "UPDATE")
	{
		%ver = getField(%line, 1);
		%size = getField(%line, 2); //In kB
		%released = getField(%line, 3);
		canvas.pushDialog(BLG_Updater);
		BLG_GUI_Updater_version.setValue(%ver);
		BLG_GUI_Updater_size.setValue(%size @ " kB");
		BLG_GUI_Updater_released.setValue(%released);
		BLG_GAU.version = %ver;
	}
}

BLG_GAU_TCP.getVersion();

function BLG_GAU::downloadVersion(%this, %version) {
	%tcp = new TCPObject(BLG_GAU_Downloader) {
		version = %version;
	};

	canvas.pushDialog(BLG_Updater);

	%tcp.connect("api.blockland.zivle.com:80");
}

function BLG_GAU::confirmUpdate(%this) {
	%this.downloadVersion(%this, BLG_GUI_Updater_version.getValue());
}

function BLG_GAU_Downloader::onConnected(%this) {
	%data = "version=" @ %this.version @ "&cur=" @ BLG.internalVersion;

	%tString = "POST /update.php HTTP/1.1\n";
	%tString = %tString @ "Host: api.blockland.zivle.com\n";
	%tString = %tString @ "Cookie: " @ BLG_Connection.cookie @ "\n";
	%tString = %tString @ "Content-Type: application/x-www-form-urlencoded\n";
	%tString = %tString @ "Content-Length: " @ strLen(%data) @ "\n";
	%tString = %tString @ "\n" @ %data @ "\n";

	%this.send(%tString);
}

function BLG_GAU_Downloader::onLine(%this, %line) {
	if(strPos(%line, "Content-Length:") >= 0)
		%this.size = getWord(%line, 1);
	
	//if(%line $= "NOTFOUND")
	//	messageBoxOk("Updater Error", "The updater had an internal error. The file was not found.");

	if(%line $= "")
		%this.setBinarySize(%this.size);
}

function BLG_GAU_Downloader::onBinChunk(%this, %chunk) {
	if(%this.startTime $= "")
		%this.startTime = getSimTime();

	if(%chunk < %this.size)
	{
		BLG_GUI_Updater_progress.setValue(%chunk/%this.size);
		BLG_GUI_Updater_progressText.setValue(mFloor((%chunk/%this.size)*100));
		BLG_GUI_Updater_downloaded.setValue(mFloatLength(%chunk/1024, 2) @ " kB");
		BLG_GUI_Updater_time.setValue(mFloatLength(%chunk/(getSimTime()-%this.startTime), 1) @ " kB/s");
	}
	else
	{
		%this.disconnect();

		BLG_GUI_Updater_progress.setValue(1);
		BLG_GUI_Updater_progressText.setValue("100%");
		BLG_GUI_Updater_downloaded.setValue(mFloatLength(%this.size/1024, 2) @ " kB");
		BLG_GUI_Updater_time.setValue("Done!");

		if(isWriteableFilename("Add-Ons/System_BlocklandGlass.zip") && isWriteableFilename("config/BLG/updater/temp.zip"))
		{
			%this.saveBufferToFile("config/BLG/updater/temp.zip"); //Just incase we crash for some reason, we dont leave some sort of corrupted shit
			
			fileCopy("config/BLG/updater/temp.zip", "Add-Ons/System_BlocklandGlass.zip");
			fileDelete("config/BLG/updater/temp.zip");

			messageBoxOkCancel("Update Downloaded", "To finish installing the update of Blockland Glass, Blockland must now shut-down.", "quit();", "canvas.popDialog(BLG_Updater);");
		}
		else
			messageBoxOkCancel("Read-Only!", "It seems that your Add-Ons and config folders are Read-Only. You must conduct the update yourself.", "canvas.popDialog(BLG_Updater);", "canvas.popDialog(BLG_Updater);");

	}
}