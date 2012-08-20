//Blockland Glass Client Implementation
exec("./core.cs");
if(!BLG.start("client")) return;

activatePackage(BLG_GSC_Package);
activatePackage(BLG_GKC_Package);
activatePackage(BLG_GDC_Package);
activatePackage(BLG_HUDC_Package);
activatePackage(BLG_GNS_Package);
activatePackage(BLG_GRSC);
activatePackage(BLG_Updater_Package);

%mm = new GuiTextCtrl(BLG_Version) {
	profile = "BLG_VersionTextProfile";
	horizSizing = "center";
	vertSizing = "bottom";
	position = "237 0";
	extent = "165 16";
	minExtent = "8 2";
	visible = "1";
	text = "BLG Version: " @ BLG.externalVersion;
	maxLength = "255";
};
MainMenuGui.add(%mm);

function clientCmdBLG_Handshake(%version, %internalVersion) {
	BLG.debug("Server version id: " @ %version);
	commandToServer('BLG_HandshakeResponse', canvas.getExtent());
}

package BLG_Client {
	function onExit() {
		%r = "config/BLG/client/cache/*";
		%file = findFirstFile(%r);
		while(%file !$= "") {
			fileDelete(%file);
			%file = findNextFile(%r);
		}
		parent::onExit();
	}

	function MM_AuthBar::blinkSuccess(%this) {
		if(BLG.firstRun) {
			messageBoxOk("BLG First-run Placeholder", "This is a place holder for the welcome GUI of BLG2. If you're seeing this, you're in the Alpha. Welcome.");
		}
		parent::blinkSuccess(%this);
	}
};
activatePackage(BLG_Client);

//Thank-you, dearest Iban
$cArg[8, mFloor($cArgs[8])] = "BLG" TAB BLG.internalVersion TAB BLG.versionId;
$cArgs[8]++;
exec("./Support_ModVersion.cs");