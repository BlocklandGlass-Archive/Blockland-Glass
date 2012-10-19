//================================================
// Title: Glass HUD Client 
//================================================

if(!isObject(BLG_HUDC)) {
	new ScriptObject(BLG_HUDC);
	BLG_HUDC.serverCache = new ScriptGroup() {
		items = 0;
	};
}

//================================================
// BLG_HUDC
//================================================

function BLG_HUDC::registerNewHUDValue(%this, %type, %title, %value) {
	if(%this.serverCache.itemTitle[%title] $= "") {
		if(%type !$= "text" && %type !$= "progressbar") {
			return;
		}

		%this.serverCache.item[%this.serverCache.items] = %title NL %type NL %value;
		%this.serverCache.itemTitle[%title] = %this.serverCache.items;
		%this.serverCache.items++;
	}
}

function BLG_HUDC::updateValue(%this, %title, %value) {
	%id = %this.serverCache.itemTitle[%title];
	%data = %this.serverCache.item[%i];

	if(getLine(%data, 1) $= "text") {
		%this.serverCache.itemValObj[%i].setValue(%value);
	} else if(getLine(%data, 1) $= "progressbar") {
		%this.serverCache.itemValObj[%i].setValue(getLine(%value, 0));
		%this.serverCache.itemValObj[%i].getObject(0).setValue(getLine(%value, 1));
	}
}

function BLG_HUDC::draw(%this) {
	canvas.popDialog(BLG_HUD);
	BLG_HUD.deleteAll();
	if(PlayGui.isMember(BLG_HUD)) {
		PlayGui.remove(BLG_HUD);
	}
	
	%y = 435;
	if(%this.serverCache.items > 0) {
		for(%i = 0; %i < %this.serverCache.items; %i++) {
			%bar = new GuiSwatchCtrl() {
				profile = "GuiDefaultProfile";
				horizSizing = "left";
				vertSizing = "top";
				position = 430 SPC %y-(35*%i);
				extent = "200 30";
				minExtent = "8 2";
				visible = "1";
				color = "50 50 50 192";

				new GuiTextCtrl() {
					profile = "BLG_HudText";
					horizSizing = "right";
					vertSizing = "center";
					position = "5 2";
					extent = "100 25";
					minExtent = "8 2";
					visible = "1";
					text = getLine(%this.serverCache.item[%i], 0);
					maxLength = "255";
				};
	 		};

	 		if((%type = getLine(%this.serverCache.item[%i], 1)) $= "text") {
				%val = new GuiTextCtrl() {
					profile = "BLG_HudTextRight";
					horizSizing = "right";
					vertSizing = "center";
					position = "90 2";
					extent = "100 25";
					minExtent = "100 25";
					visible = "1";
					text = getLine(%this.serverCache.item[%i], 2);
					maxLength = "255";
				};
	 		} else if(%type $= "progressbar") {
				%val = new GuiProgressCtrl() {
					profile = "GuiProgressProfile";
					horizSizing = "right";
					vertSizing = "center";
					position = "90 2";
					extent = "105 25";
					minExtent = "105 25";
					visible = "1";

					new GuiTextCtrl() {
						profile = "BLG_HudTextCenter";
						horizSizing = "center";
						vertSizing = "center";
						position = "0 0";
						extent = "105 25";
						minExtent = "105 25";
						visible = "1";
						text = getLine(%this.serverCache.item[%i], 3);
						maxLength = "255";
					};
				};
				%val.setValue(getLine(%this.serverCache.item[%i], 2));
	 		}

	 		%this.serverCache.itemValObj[%i] = %val;
	 		%bar.add(%val);
	 		BLG_HUD.add(%bar);
		}

	 	canvas.pushDialog(BLG_HUD);
		PlayGui.schedule(1, "add", BLG_HUD);
	}
}

//================================================
// Networking
//================================================

function clientCmdBLG_HUD(%call, %arg1, %arg2, %arg3, %arg4) {
	if(%call $= "0") {
		BLG_HUDC.registerNewHUDValue(%arg1, %arg2, %arg3);
	} else if(%call $= "1") {
		BLG_HUDC.updateValue(%arg1, %arg2);
	} else if(%call $= "2") {
		BLG_HUDC.draw();
	}
}

//================================================
// Package
//================================================

package BLG_HUDC_Package {
	function disconnectedCleanup() {
		PlayGui.remove(BLG_HUD);

		BLG_HUD.deleteAll();
		BLG_HUDC.serverCache.delete();
		BLG_HUDC.serverCache = new ScriptGroup() {
			items = 0;
			y = 435;
		};

		parent::disconnectedCleanup();
	}

	function PlayGui::onWake(%gui) {
		parent::onWake(%gui);
		BLG_HUDC.draw();
	}
};
