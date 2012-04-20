//================================================
// Title: Glass BOS server hook 
//================================================

package BLG_BOS {
	function BlockOS_registerShortcut(%gui, %name, %img, %path) {
		if(%name $= "App Store") {
			if(!BLG.registeredBOS) {
				parent::BlockOS_registerShortcut(BLG_homeGui, "BLG", "logo", "Add-Ons/System_BlocklandGlass/image");
				BLG.registeredBOS = true;
			}
		}
		parent::BlockOS_registerShortcut(%gui, %name, %img, %path);
	}
};
activatePackage(BLG_BOS);