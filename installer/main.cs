if(!isObject(BLG_Installer)) {
	new ScriptObject(BLG_Installer) {
		steps = 3;
	};
}

Canvas.popDialog(MainMenuGui);

function BLG_Install_BG::onWake(%this) {
	if(!BLG_Install_Frame1.awoken) {
		Canvas.pushDialog(BLG_Install_Frame1);
		BLG_Install_Frame1.awoken = true;
	}
}

function BLG_Installer::openStep(%this, %step) {
	if(%this.steps >= %step) {
		Canvas.popDialog("BLG_Install_Frame" @ %step-1);
		Canvas.pushDialog("BLG_Install_Frame" @ %step);
	}

	if(%step == 3) {
		BLG_IF3_Text.setValue("Features enabled:<br><br>GUI System <color:aaaaaa>(Framework)<br><color:000000>HUD System <color:aaaaaa>(Framework)<br><color:000000>Server Rating<br>" @ (BLG_IF2_DesktopToggle.getValue() ? "BLG Desktop<br>" : "") @ (BLG_IF2_LoadingToggle.getValue() ? "New Loading Screen<br>" : "") @ "<br>Blockland must now restart to load with your options.");
	}
}

function BLG_Installer::finish(%this) {
	$BLG::Pref::InstallerHasRun = true;
	$BLG::Pref::Desktop = BLG_IF2_DesktopToggle.getValue();
	$BLG::Pref::LoadingScreen = BLG_IF2_LoadingToggle.getValue();
	quit();
}


exec("./BLG_Install_Frame1.gui");
exec("./BLG_Install_Frame2.gui");
exec("./BLG_Install_Frame3.gui");
BLG_IF1_Welcome.setValue("Welcome to Blockland Glass version " @ BLG.externalVersion @ "! Blockland Glass is a multi-purpose Add-on, so we'd like to give you options on what is enabled and disabled.<br><br>We've categorized the different features into \"Framework\" and \"Visual\". All framework is manditory, where as none of the visual features are.");

new GuiSwatchCtrl(BLG_Install_BG) {
	profile = "GuiDefaultProfile";
	horizSizing = "width";
	vertSizing = "height";
	extent = "640 480";
	position = "0 0";
	color = "127 127 127 255";
	visible = "1";
	minExtent = "8 2";
};

Canvas.pushDialog(BLG_Install_BG);
