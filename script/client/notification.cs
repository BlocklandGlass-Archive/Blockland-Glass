//================================================
// Title: Glass Notification System
//================================================

if(!isObject(BLG_GNS)) {
	new ScriptObject(BLG_GNS) {
		queueItems = -1;
	};
}

function BLG_GNS::newNotification(%this, %title, %text, %icon, %time, %command) {
	if(%time != -1 && %time < 5000) {
		%time = 5000;
	}

	if(%icon $= "") {
		%icon = "information";
	}

	%so = new ScriptObject() {
		class = BLG_Notification;

		time = %time;
		text = %text;
		title = %title;
		command = %command;
		icon = %icon;
	};

	%so.draw();
	return %so;
}

function BLG_GNS::addToQueue(%this, %action, %object) {
	if(%this.queueItems == -1) {
		%inactive = true;
		%this.queueItems = 1;
	}

	%this.queueItem[%this.queueItems] = %action TAB %object;
	%this.queueItems++;
	if(%inactive) {
		BLG_GNS.processQueue();
		echo("Restarting queue");
	}
}

function BLG_GNS::processQueue(%this, %finished) {
	%this.queueItem[0] = "";
	for(%i = 0; %i < %this.queueItems; %i++) {
		%this.queueItem[%i] = %this.queueItem[%i+1];
	}

	if(%this.queueItem[0] $= "") {
		%this.queueItems = -1;
		echo("Queue empty");
		return;
	}
	%this.queueItems--;

	%action = getField(%this.queueItem[0], 0);
	%object = getField(%this.queueItem[0], 1);
	%this.currentQueue = %action TAB %object;

	switch$(%action) {
		case "add":
			%object.schedule(100, finalize, %object.gui);

		case "remove":
			%object.pop();
	}
}

function BLG_Notification::draw(%this) {
	%res = getWords($pref::Video::windowedRes, 0, 1);
	BLG_GNS.open++;
	%gui = new GuiSwatchCtrl() {
		profile = "GuiDefaultProfile";
		extent = "200 50";
		position = getWord(%res, 0) SPC getWord(%res, 1) - 55*BLG_GNS.open;
		color = "0 0 0 200";	
		new GuiBitmapCtrl() {
			position = "5 5";
			extent = "14 14";
			bitmap = $RTB::Path@"images/icons/"@%this.icon;
		};
		new GuiMLTextCtrl() {
			position = "24 5";
			extent = "171 14"; 
			text = "<shadow:2:2><shadowcolor:00000066><color:EEEEEE><font:Verdana Bold:15>" @ %this.title;
			selectable = false;
		};
		new GuiMLTextCtrl() {
			position = "24 21";
			extent = "171 12";
			text = "<shadow:2:2><shadowcolor:00000066><color:DDDDDD><font:Verdana:12>" @ %this.text;
			selectable = false;
		};
		new GuiBitmapButtonCtrl() {
			position = "0 0";
			extent = "200 50";
			text = " ";
			command = "BLG_GNS.addToQueue(\"remove\", " @ %this @ ");" @ %this.command;
		};
	};
	%this.gui = %gui;
	MainMenuGui.add(%gui);
	BLG_GNS.addToQueue("add", %this);
}

function BLG_Notification::finalize(%this, %gui) {
	%this.action = "add";

	%res = getWords($pref::Video::windowedRes, 0, 1);
	%msg = %gui.getObject(2).extent;

	%gui.getObject(3).extent = %gui.extent = "200" SPC 26+getWord(%msg, 1);
	%gui.obj = %this;

	BLG_GNS.slot[BLG_GNS.notifications++] = %gui;
	BLG_GNS.height += 31+getWord(%msg, 1);
	%this.id = BLG_GNS.notifications;

	%gui.position = getWord(%gui.position, 0) SPC getWord(%res, 1)-(BLG_GNS.height);

	BLG_CAS.newAnimation(%gui).setPosition(getWord(%res, 0)-205 SPC getWord(%res, 1) - (BLG_GNS.height)).setDuration(250).setColor(%gui.color).setFinishHandle(%this @ ".actionFinished").start();
}

function BLG_Notification::pop(%this) {
	%this.action = "remove";
	%pos = %this.gui.position;
	BLG_CAS.newAnimation(%this.gui).setPosition(getWord(%pos, 0)+205 SPC getWord(%pos, 1)).setDuration(250).setColor("0 0 0 0").setFinishHandle(%this @ ".actionFinished").start();
}

function BLG_Notification::actionFinished(%this, %obj) {
	BLG.debug(%obj);
	if(%this.action $= "remove") {
		for(%i = %this.id+1; %i <= BLG_GNS.notifications; %i++) {
			%gui = BLG_GNS.slot[%i];
			%pos = %gui.position;
			BLG_CAS.newAnimation(%gui).setPosition(getWord(%pos, 0) SPC getWord(%pos, 1)+getWord(%obj.extent, 1)+5).setColor(%gui.color).setDuration(250).start();
			BLG_GNS.slot[%i-1] = %gui;
			%gui.obj.id--;
		}

		BLG_GNS.notifications--;
		BLG_GNS.height -= getWord(%obj.extent, 1);
		%obj.delete();
		BLG_GNS.schedule(250, processQueue);
		return;
	} else if(%this.action $= "add") {
		if(%this.time > -1) {
			BLG_GNS.schedule(%this.time, addToQueue, "remove", %this);
		}
	}

	BLG_GNS.processQueue();
}

package BLG_GNS_Package {
	function RTBCC_NotificationManager::push(%this,%title,%message,%icon,%key,%holdTime) {
		//parent::push(%this,%title,%message,%icon,%key,%holdTime);
		BLG_GNS.newNotification(%title, %message, %icon, %holdTime, "RTB_Overlay.fadeIn();");
	}

	function MM_AuthBar::blinkSuccess(%this) {
		parent::blinkSuccess(%this);
		BLG_GNS.newNotification("Welcome to BLG!", "Welcome to Blockland Glass! Click on me to access BLG, or simply press \"shift tab\".", "", -1, "RTB_Overlay.fadeIn();");
	}
};