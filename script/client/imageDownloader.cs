//================================================
// Title: Glass Image Downloader Client
//================================================

if(!isObject(BLG_IDC)) {
	new ScriptObject(BLG_IDC);
}

//================================================
// BLG_IDC
//================================================

function BLG_IDC::downloadFile(%this, %host, %path, %file) {
	%tcp = new TCPObject(BLG_IDC_Downloader) {
		host = %host;
		path = %path;
		file = %file;
	};

	%tcp.connect(%host @ ":80");
}

function BLG_IDC::downloadLoadingPhaseFile(%this, %host, %path, %file) {
	%tcp = new TCPObject(BLG_IDC_Downloader) {
		host = %host;
		path = %path;
		file = %file;

		loadPhase = true;
	};

	%tcp.connect(%host @ ":80");
}

//================================================
// BLG_IDC_Downloader
//================================================

function BLG_IDC_Downloader::onConnected(%this) {
	BLG.debug("Downloader connected");
	%msg = "GET " @ %this.path @ " HTTP/1.1";
	%msg = %msg NL "Host: " @ %this.host;
	%msg = %msg NL "User-agent: BLG/" @ BLG.internalVersion;
	%msg = %msg NL "\n";
	%this.send(%msg);
}

function BLG_IDC_Downloader::onLine(%this, %line) {
	BLG.debug("Line:" SPC %line);
	if(strPos(%line, "Content-Length:") >= 0)
		%this.size = getWord(%line, 1);

	if(%line $= "") {
		%this.setBinarySize(%this.size);
	}
}

function BLG_IDC_Downloader::onBinChunk(%this, %chunk) {
	BLG.debug("Chunk " @ %chunk);
	if(%chunk < %this.size) {
		//Not done
	} else {
		%this.saveBufferToFile(%this.file);
		if(%this.loadPhase) {
			BLG_IDC.nextPhaseFile();
		}

		%this.disconnect();
		%this.schedule(1000, delete);
	}
}

function BLG_IDC_Downloader::onDisconnect(%this) {
	BLG.debug("Downloader disconnected");
}

//================================================
// MissionPreparePhaseBLG2
//================================================

function clientCmdMissionPreparePhaseBLG2(%files) {
	BLG_IDC.files = %files;
	BLG_IDC.filesDownloaded = 0;
	BLG_IDC.loading = true;
	LoadingProgressTxt.setValue("Downloading BLG Images");
	LoadingProgress.setValue(0);
	commandtoserver('MissionPreparePhaseBLG2Ack');
	echo("*** Starting BLG Image/Font Download Phase - (" @ %files @ " files)");
}

function clientCmdBLG_IDC_fileData(%webData, %type, %name) {
	BLG_IDC.addFile(%webData, %type, %name);
}

function clientCmdBLG_IDC_finishedFileData() {
	BLG_IDC.filesDownloaded = -1;
	BLG_IDC.nextPhaseFile();
}

function BLG_IDC::nextPhaseFile(%this) {
	%this.filesDownloaded++;
	LoadingProgress.setValue(%this.filesDownloaded/%this.files);
	BLG.debug("nextPhaseFile " @ %this.filesDownloaded);

	if((%fileData = %this.fileData[%this.filesDownloaded]) !$= "") {
		BLG.debug("Downloading file:" SPC %filedata);
		%this.downloadLoadingPhaseFile(getField(%fileData, 0), getField(%fileData, 1), getField(%fileData, 2) $= "image" ? "config/BLG/images/" @ getField(%fileData, 3) : "base/client/ui/cache/" @ getField(%fileData, 3));
	} else {
		//We've finished the queue
		commandToServer('MissionPreparePhaseBLG2Complete');
	}
}

function BLG_IDC::addFile(%this, %webData, %type, %name) {
	%this.fileData[%this.fileDatas += 0] = %webData TAB %type TAB %name;
	BLG.debug("File data " @ %this.fileData[%this.fileDatas]);
	%this.fileDatas++;
}