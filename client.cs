//Blockland Glass Version 1.0 Client Implementation
exec("./core.cs");

BLG.start("client");

package BLG_Client {
	function MM_AuthBar::blinkSuccess(%this) {
		if(BLG.firstRun) {
			messageBoxOk("BLG First-run Placeholder", "This is a place holder for the welcome GUI of BLG2. If you're seeing this, you're in the Alpha. Welcome.<br><br>Btw, try CTRL + SPACE");
		}
		parent::blinkSuccess(%this);
	}
};
activatePackage(BLG_Client);

//Thank-you, dearest Iban
$cArg[8, mFloor($cArgs[8])] = "BLG" TAB BLG.internalVersion;
$cArgs[8]++;
exec("./Support_ModVersion.cs");