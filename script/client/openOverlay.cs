//================================================
// Title: Glass Open Overlay
//================================================

if(!isObject(BLG_GOO)) {
	new ScriptObject(BLG_GOO);
}

//================================================
// BLG_GOO
//================================================
GlobalActionMap.bind("keyboard", "CTRL SPACE", "BLG_ToggleOverlay");

function BLG_ToggleOverlay(%down) {
	BLG.debug("Toggle overlay [" @ %down @ "]");
	if(!%down) {
		%this = BLG_GOO;
		%this.overlayOpen += 0;
		if(!%this.overlayOpen) {
			canvas.pushDialog(BLG_Overlay);
			%this.overlayOpen = true;
		} else {
			canvas.popDialog(BLG_Overlay);
			%this.overlayOpen = false;
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
			}
		}
		%this.type[%gui] = "instance";
	} else {
		for(%i = 0; %i < %gui.getCount(); %i++) {
			%obj = %gui.getObject(%i);
			if(%obj.getClassName() $= "GuiWindowCtrl") {
				%this.window[%gui] = %obj;
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

	BLG_Overlay.add(%ins);

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
	BLG_Overlay.remove(%instance);
}

function BLG_GOO::openGui(%this, %gui) {
	BLG_Overlay.add(%this.window[%gui]);
}

function BLG_GOO::closeGui(%this, %gui) {
	%win = %this.window[%gui];
	if(BLG_Overlay.isMember(%win)) {
		BLG_Overlay.remove(%win);
	}
}

function BLG_GOO::newIcon(%this, %name, %gui, %icon) {
	%this.iconY += 0;
	%this.iconX += 0;
	%icon = new GuiSwatchCtrl() {
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = %this.iconX SPC %this.iconY;
		extent = "80 85";
		minExtent = "8 2";
		visible = "1";
		color = "0 0 0 0";
		BLG_GUI = %gui;
		className = "BLG_Icon";

		new GuiTextCtrl() {
			profile = "GuiCenterTextProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "0 67";
			extent = "80 18";
			minExtent = "75 2";
			visible = "1";
			text = "\c3" @ %name;
			maxLength = "255";
		};

		new GuiBitmapButtonCtrl() {
			profile = "GuiDefaultProfile";
			horizSizing = "center";
			vertSizing = "bottom";
			position = "15 15";
			extent = "50 50";
			minExtent = "8 2";
			visible = "1";
			wrap = "0";
			lockAspectRatio = "0";
			alignLeft = "0";
			overflowImage = "0";
			keepCached = "0";
			text = "";
			bitmap = %icon;
			command = %this.type[%gui] $= "instance" ? "BLG_GOO.newInstance(" @ %gui @ ");" : "BLG_GOO.openGui(" @ %gui @ ");";
		};
	};

	BLG_Overlay_Swatch.add(%icon);

	if(%this.iconX/getWord(canvas.getExtent(), 0) == 1) {
		%this.iconX = 0;
		%this.iconY += 90;
	} else {
		%this.iconX += 80;
	}
}

BLG_GOO.newIcon("Home", BLG_Home, "Add-Ons/System_BlocklandGlass/image/logo");
BLG_GOO.registerGui(BLG_Home, false);
BLG_GOO.registerGui(BLG_RemoteControl, true);
BLG_GOO.openGui(BLG_Home);