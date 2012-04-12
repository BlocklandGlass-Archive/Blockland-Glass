//================================================
// Title: Glass Keybind Client 
//================================================

$remapDivision[$remapCount] = "Blockland Glass";
$remapName[$remapCount] = "Open Keybind Gui";
$remapCmd[$remapCount] = "BLG_GKC.openBindGui";
$remapCount++;

if(!isObject(BLG_GKC)) {
	new ScriptObject(BLG_GKC) {
		binds = 0;
	};
}

function BLG_GKC::loadData(%this) {
	%fo = new FileObject();
	%fo.openForRead("config/BLG/client/binds.txt");
	while(!%fo.isEOF()) {
		%line = %fo.readLine();
		if(BLG_GKC.bindName[getField(%line, 0)] $= "") {
			BLG_GKC.newBind(getField(%line, 0));
			BLG_GKC.setBind(getField(%line, 0), getField(%line, 1), getField(%line, 2));
		}
	}
	%fo.close();
	%fo.delete();
}

function BLG_GKC::saveData(%this) {
	%fo = new FileObject();
	%fo.openForWrite("config/BLG/client/binds.txt");
	for(%i = 0; %i < %this.binds; %i++) {
		%fo.writeLine(%this.bind[%i] TAB getField(%this.bindData[%i], 0) TAB getField(%this.bindData[%i], 1));
	}
	%fo.close();
	%fo.delete();
}

function BLG_GKC::setBind(%this, %name, %device, %key) {
	%this.bindData[%this.bindName[%name]] = %device TAB %key;
}

function BLG_GKC::newBind(%this, %name) {
	%this.bind[%this.binds] = %name;
	%this.bindName[%name] = %this.binds;
	eval("function BLG_BindCallback" @ %this.binds @ "(%t){BLG_GKC.bindCallback(" @ %this.binds @ ", %t);}");
	%this.binds++;
}

function BLG_GKC::activateBinds(%this) {
	for(%i = 0; %i < %this.binds; %i++) {
		GlobalActionMap.bind(getField(%this.bindData[%i], 0), getField(%this.bindData[%i], 1), "BLG_BindCallback" @ %i);
	}
}

function BLG_GKC::deactivateBinds(%this) {
	for(%i = 0; %i < %this.binds; %i++) {
		GlobalActionMap.unbind(getField(%this.bindData[%i], 0), getField(%this.bindData[%i], 1));
	}
}

function BLG_GKC::bindCallback(%this, %id, %tog) {
	if(%tog) {
		%data = %this.bindData[%id];
		for(%i = 0; %i < %this.binds; %i++) {
			if(%this.bindData[%i] $= %data) {
				commandtoserver('BLG_BindCallback', %this.bindName[%i]);
			}
		}
	}
}

function BLG_GKC::pushSetBindGui(%this) {
	BLG_keybindGui.text = "Set New Keybinds";
	BLG_keybindGui.closeCommand = "BLG_GKC.checkFinished();";
	BLG_keybindList.clear();
	for(%i = 0; %i < %this.binds; %i++) {
		if(getField(%this.bindData[%i], 1) $= "") {
			BLG_keybindList.addRow(BLG_keybindList.rowCount(), %this.bind[%i] TAB "\c5" @ getField(%this.bindData[%i], 1));
		}
	}
	BLG_keybindGui.mode = "new";
	canvas.pushDialog(BLG_keybindGui);
}

function BLG_GKC::openBindGui(%this) {
	BLG_keybindGui.text = "Edit Keybinds";
	BLG_keybindGui.closeCommand = "BLG_GKC.checkFinished();";
	BLG_keybindList.clear();
	for(%i = 0; %i < %this.binds; %i++) {
		BLG_keybindList.addRow(BLG_keybindList.rowCount(), %this.bind[%i] TAB "\c5" @ getField(%this.bindData[%i], 1));
	}
	BLG_keybindGui.mode = "edit";
	canvas.pushDialog();
}

function BLG_GKC::checkFinished() {
	for(%i = 0; %i < %this.binds; %i++) {
		if(getField(%this.bindData[%i], 1) $= "") {
			messageBoxOk("Whoops!", "You forgot to bind a bind!");
			return;
		}
	}

	canvas.popDialog(BLG_keybindGui);
}

function BLG_keybindList::onSelect(%this, %id, %text) {
	%name = getField(%text, 0);
	canvas.pushDialog(BLG_remapGui);
	BLG_remapGuiText.setValue("Select Bind For: " @ %name);
	BLG_GKC.currentRemap = %name;

	%ctrl = new GuiInputCtrl(BLG_Remapper);
	BLG_remapGui.add(%ctrl);
	%ctrl.makeFirstResponder(1);
}

function BLG_Remapper::onInputEvent(%this, %device, %key) {
	if(%device $= "mouse0") {
		return;
	}

	%disallow = "wasdbq1234567890";

	if(strPos(%key, %disallow) != -1) {
		messageBoxOk("Key Disallowed");
		canvas.popDialog(BLG_remapGui);
		%this.delete()
		return;
	}


	BLG_GKC.setBind(BLG_GKC.currentRemap, %device, %key);
	BLG_GKC.saveData();
	BLG_keybindList.clear();
	if(BLG_GKC.mode $= "edit") {
		for(%i = 0; %i < BLG_GKC.binds; %i++) {
			BLG_keybindList.addRow(BLG_keybindList.rowCount(), BLG_GKC.bind[%i] TAB "\c5" @ getField(BLG_GKC.bindData[%i], 1));
		}
	} else {
		for(%i = 0; %i < BLG_GKC.binds; %i++) {
			if(BLG_GKC.bindData[%i]) {
				BLG_keybindList.addRow(BLG_keybindList.rowCount(), BLG_GKC.bind[%i] TAB "\c5" @ getField(BLG_GKC.bindData[%i], 1));
			}
		}
	}
	%this.delete();
}

function clientcmdBLG_requireBind(%name) {
	if(BLG_GKC.bindName[%name] !$= "") {
		return;
	}
	BLG_GKC.newBind(%name);
}

package BLG_GKC_Package {
	function Canvas::pushDialog(%canvas, %gui) {
		%ret = parent::pushDialog(%canvas, %gui);

		if(BLG_GKC.playGui && %canvas.getCount() != 2) {
			BLG_GKC.deactivateBinds();
		}

		return %ret;
	}

	function Canvas::popDialog(%canvas, %gui) {
		%ret = parent::popDialog(%canvas, %gui);

		if(BLG_GKC.playGui) {
			if(%canvas.getCount() == 2) {
				BLG_GKC.activateBinds();
			}
		}

		return %ret;
	}

	function PlayGui::onWake(%gui) {
		parent::onWake(%gui);
		if(!BLG_GKC.playGui) {
			BLG_GKC.activateBinds();
		}

		BLG_GKC.playGui = true;

		for(%i = 0; %i < BLG_GKC.binds; %i++) {
			if(BLG_GKC.bindData[%i] $= "") {
				BLG_GKC.pushSetBindGui();
				break;
			}
		}
	}

	function PlayGui::onSleep(%gui) {
		parent::onSleep(%gui);
		BLG_GKC.playGui = false;
		BLG_GKC.deactivateBinds();
	}
};
activatePackage(BLG_GKC_Package);

BLG_GKC.loadData();