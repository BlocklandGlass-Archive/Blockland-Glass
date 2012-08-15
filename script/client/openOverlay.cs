//================================================
// Title: Glass Open Overlay
//================================================

if(!isObject(BLG_GOO)) {
	new ScriptObject(BLG_GOO);
}

if(!isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
	GlobalActionMap.bind("keyboard", "CTRL TAB","BLG_toggleOverlay");
	BLG_GOO.overlay = BLG_Overlay;
} else {
	BLG_GOO.overlay = RTB_Overlay;
}

function BLG_toggleOverlay(%down) {
	if(!%down) {
		%this = BLG_GOO;
		%this.open = !%this.open;
		if(%this.open) {
			canvas.pushDialog(BLG_Overlay);
		} else {
			canvas.popDialog(BLG_Overlay);
		}
	}
}

function BLG_GOO::registerGui(%this, %gui, %replicable) {
	%gui.BLG_replicable = %replicable;
	%replicable += 0;

	if(%replicable) {
		%file = "config/BLG/client/cache/overlay/" @ sha1(%gui.getName()) @ ".gui";
		for(%i = 0; %i < %gui.getCount(); %i++) {
			%obj = %gui.getObject(%i);
			if(%obj.getClassName() $= "GuiWindowCtrl") {
				%obj.save(%file);
				%this.replicate[%gui] = %file;
				%this.windowName[%gui] = %obj.getName();
				break;
			}
		}
		%this.type[%gui] = "instance";
	} else {
		for(%i = 0; %i < %gui.getCount(); %i++) {
			%obj = %gui.getObject(%i);
			if(%obj.getClassName() $= "GuiWindowCtrl") {
				%this.window[%gui] = %obj;
				break;
			}
		}
		%this.type[%gui] = "window";
	}
}

function BLG_GOO::newInstance(%this, %gui) {
	%this.instanceId++;
	%fo = new FileObject();
	%fo.openForRead(%this.replicate[%gui]);
	while(!%fo.isEOF()) {
		%lastline = %line;
		%line = %fo.readLine();
		if(!%started && %lastline $= "//--- OBJECT WRITE BEGIN ---") {
			%started = true;
			%script = "return " @ %line;
		} else if(%started) {
			%script = %script @ strReplace(%line, %this.windowName[%gui], "BLG_GOO.instanceObj" @ %this.instanceId);
		}
	}
	%fo.close();
	%fo.delete();

	%this.instanceObj[%this.instanceId] = %ins = eval(%script);
	%this.mapObject(%ins, %ins);

	%this.overlay.add(%ins);

	return %ins;
}

function BLG_GOO::mapObject(%this, %base, %obj) {
	BLG.debug("Mapping Object " @ %obj @ " (Base: " @ %base @ ")");
	for(%i = 0; %i < %obj.getCount()+0; %i++) {
		%o = %obj.getObject(%i);
		if(%o.BLG_InstanceTag !$= "") {
			%base.part[%o.BLG_InstanceTag] = %o;
			BLG.debug("Has tag [" @ %o.BLG_InstanceTag @ "]");
		}
		%this.mapObject(%base, %o);
	}
}

function BLG_GOO::removeInstance(%this, %instance) {
	%this.overlay.remove(%instance);
	%instance.delete();
}

function BLG_GOO::openGui(%this, %gui) {
	%this.overlay.add(%this.window[%gui]);
}

function BLG_GOO::closeGui(%this, %gui) {
	%win = %this.window[%gui];
	if(%this.overlay.isMember(%win)) {
		%this.overlay.remove(%win);
	}
}

//package BLG_GOO {
	//function RTB_toggleOverlay(%trigger) {
	//	BLG_toggleOverlay();
	//}
//};
//`activatePackage(BLG_GOO);

BLG_GOO.registerGui(BLG_Home, false);
BLG_GOO.registerGui(BLG_RemoteControl, true);
BLG_GOO.registerGui(BLG_SelectServer, false);
BLG_GOO.openGui(BLG_Home);