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
	%msg = "GET " @ %file @ " HTTP/1.1";
	%msg = %msg NL "Host: " @ %this.host;
	%msg = %msg NL "User-agent: BLG/" @ BLG.internalVersion;
	%msg = %msg NL "\n";
	%this.send(%msg);
}

function BLG_IDC_Downloader::onLine(%this) {
	if(strPos(%line, "Content-Length:") >= 0)
		%this.size = getWord(%line, 1);

	if(%line $= "" && %this.isFile) {
		%this.setBinarySize(%this.size);
	}
}

function BLG_IDC_Downloader::onBinChunk(%this, %chunk) {
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
	echo("*** Starting BLG Image/Font Download Phase - (" @ %parts @ " parts)");
}

function BLG_IDC::nextPhaseFile(%this) {
	%this.filesDownloaded++;
	LoadingProgress.setValue(%this.filesDownloaded/%this.files);

	if((%fileData = %this.fileData[%this.filesDownloaded]) !$= "") {
		%this.downloadLoadingPhaseFile(getField(%fileData, 0), getField(%fileData, 1), getField(%fileData, 2) $= "image" ? "config/BLG/" @ getField(%fileData, 3) : "base/client/ui/cache/" @ getField(%fileData, 3));
	} else {
		//We've finished the queue
		commandToServer('MissionPreparePhaseBLG2Complete');
	}
}

function BLG_IDC::addFile(%this, %host, %path, %type, %name) {
	%this.fileData[%this.fileDatas += 0] = %host TAB %path TAB %type TAB %name;
	%this.fileDatas++;
}