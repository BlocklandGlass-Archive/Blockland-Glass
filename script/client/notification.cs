//================================================
// Title: Glass Notification System
//================================================

if(!isObject(BLG_GNS)) {
	new ScriptObject(BLG_GNS) {
		queueItems = -1;
	};
}

function BLG_GNS::newNotification(%this, %title, %text, %icon, %time, %key, %command, %closeOnOverlay) {
	if(isFunction(clientCmdBlota_MakeNotification)) {
		switch(getRandom(1, 4)) {
			case 1:
				%col = "red";
			case 2:
				%col = "blue";
			case 3:
				%col = "green";
			case 4:
				%col = "yellow";
		}

		clientCmdBlota_MakeNotification(%title, %text, %col, %time, %key, "large", true);
		return;
	}

	echo("Time: [" @ %time @ "]");
	if(BLG.getOverlay().isAwake()) {
		return;
	}

	if(%time $= "") {
    	%time = 3000;
	}

	if(%time != 0 && %time < 3000) {
		%time = 3000;
	}

	if(%holdTime < 0 && RTBCO_getPref("CC::StickyNotifications")) {
    	%time = 0;	
    }
	echo("Modded: " @ %time);

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
		key = %this.key;

		overlay = %closeOnOverlay;
	};

	%this.key[%key] = %so;

	%so.draw();
	return %so;
}

function BLG_GNS::updateNotification(%this, %key, %title, %text, %icon, %time) {
	if(%time $= "") {
    	%time = 3000;
	}

	if(%time != 0 && %time < 3000) {
		%time = 3000;
	}
	echo("Modded: " @ %time);

	if(%icon $= "") {
		%icon = "information";
	}

	%this.key[%key].gui.getObject(0).setBitmap($RTB::Path@"images/icons/"@%icon);
	%this.key[%key].gui.getObject(1).setText("<shadow:2:2><shadowcolor:00000066><color:EEEEEE><font:Verdana Bold:15>" @ %title);
	%this.key[%key].gui.getObject(2).setText("<shadow:2:2><shadowcolor:00000066><color:DDDDDD><font:Verdana:12>" @ %text);

	cancel(%this.key[%key].sched);
	%this.key[%key].sched = BLG_GNS.schedule(%time, addToQueue, "remove", %this.key[%key]);
}

function BLG_GNS::addToQueue(%this, %action, %object) {
	if(%this.queueItems == -1) {
		%inactive = true;
		%this.queueItems = 1;
	}

	%this.queueItem[%this.queueItems] = %action TAB %object;
	%this.queueItems++;
	BLG_GNS.processQueue();
}

function BLG_GNS::processQueue(%this, %taskId) {
	if(%this.curTaskId !$= "") {
		if(%this.curTaskId != %taskId) {
			return;
		} else {
			%this.curTaskId = "";
		}
	}

	%this.queueItem[0] = "";
	for(%i = 0; %i < %this.queueItems; %i++) {
		%this.queueItem[%i] = %this.queueItem[%i+1];
	}

	%this.queueItems--;

	if(%this.queueItem[0] $= "") {
		%this.queueItems = -1;
		BLG.debug("Queue empty");
		return;
	}

	%action = getField(%this.queueItem[0], 0);
	%object = getField(%this.queueItem[0], 1);
	%this.currentQueue = %action TAB %object;

	switch$(%action) {
		case "add":
			%this.curTaskId = %this.taskId++;
			%object.schedule(100, finalize, %object.gui, %this.curTaskId);

		case "remove":
			%this.curTaskId = %this.taskId++;
			%object.pop(%this.curTaskId);
	}
}

function BLG_Notification::draw(%this) {
	%res = getRes();
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
	if(Canvas.getContent().getName() $= "PlayGui")
		PlayGui.add(%gui);
	else
		MainMenuGui.add(%gui);
	
	BLG_GNS.addToQueue("add", %this);
}

function BLG_Notification::finalize(%this, %gui, %tid) {
	%this.tid = %tid;
	%this.action = "add";

	%res = getRes();
	%msg = %gui.getObject(2).extent;

	%gui.getObject(3).extent = %gui.extent = "200" SPC 26+getWord(%msg, 1);
	%gui.obj = %this;

	BLG_GNS.slot[BLG_GNS.notifications++] = %gui;
	BLG_GNS.height += 31+getWord(%msg, 1);
	%this.id = BLG_GNS.notifications;

	%gui.position = getWord(%gui.position, 0) SPC getWord(%res, 1)-(BLG_GNS.height);

	BLG_CAS.newAnimation(%gui).setPosition(getWord(%res, 0)-205 SPC getWord(%res, 1) - (BLG_GNS.height)).setDuration(250).setColor(%gui.color).setFinishHandle(%this @ ".actionFinished").start();
}

function BLG_Notification::pop(%this, %tid) {
	%this.key[%this.key] = "";
	%this.tid = %tid;
	%this.action = "remove";
	BLG_CAS.newAnimation(%this.gui).setDuration(350).setColor("0 0 0 0").setFinishHandle(%this @ ".actionFinished").start();
	if(strPos(%this.text, "has just sent you a message.") == 0) {
		BLG_GNS.msgs[%this.title] = 0;
	}
}

function BLG_Notification::actionFinished(%this, %obj) {
	if(%this.action $= "remove") {
		for(%i = %this.id+1; %i <= BLG_GNS.notifications; %i++) {
			%gui = BLG_GNS.slot[%i];
			%pos = %gui.position;
			BLG_CAS.newAnimation(%gui).setPosition(getWord(%pos, 0) SPC getWord(%pos, 1)+getWord(%obj.extent, 1)+5).setColor(%gui.color).setDuration(100).start();
			BLG_GNS.slot[%i-1] = %gui;
			%gui.obj.id--;
		}

		BLG_GNS.notifications--;
		BLG_GNS.height -= getWord(%obj.extent, 1)+5;
		if(BLG_GNS.height < 0) {
			BLG_GNS.height = 0;
		}
		%obj.obj.delete();
		%obj.delete();
		BLG_GNS.schedule(100, processQueue, %this.tid);
		return;
	} else if(%this.action $= "add") {
		if(%this.time > -1) {
			%this.sched = BLG_GNS.schedule(%this.time, addToQueue, "remove", %this);
		}
	} else {
		return;
	}

	%tid = %this.tid;
	%this.tid = "";
	BLG_GNS.processQueue(%tid);
}

package BLG_GNS_Package {
	function RTBCC_NotificationManager::push(%this,%title,%message,%icon,%key,%holdTime) {
		//parent::push(%this,%title,%message,%icon,%key,%holdTime);
		if($blota::RTBNotif) {
			return parent::push(%this,%title,%message,%icon,%key,%holdTime);
		}

		if(strPos(%message, "has just sent you a message.") == 0) {
			BLG_GNS.msgs[%title]++;
			if(BLG_GNS.msgs[%title] > 1) {
				BLG_GNS.updateNotification(%key, %title, "has sent you <font:Verdana Bold:12>" @ BLG_GNS.msgs[%title] @ " <font:Verdana:12>messages.", "comments");
				return;
			}
		}
		BLG_GNS.newNotification(%title, %message, %icon, %holdTime, %key, "BLG.getOverlay().fadeIn();", true);
	}

	function RTB_Overlay::onWake(%this) {
		parent::onWake(%this);
		for(%i = 0; %i < BLG_GNS.notifications; %i++) {
			if(BLG_GNS.slot[%i+1].obj.overlay) {
				BLG_GNS.addToQueue("remove", BLG_GNS.slot[%i+1].obj);
			}
		}
	}

	function MM_AuthBar::blinkSuccess(%this) {
		parent::blinkSuccess(%this);
		if(!$BLG::Notified) {
			BLG_GNS.newNotification("Welcome to BLG!", "Welcome to Blockland Glass! Click on me to access BLG, or simply press \"shift tab\".", "", 0, "", "BLG.getOverlay().fadeIn();", true);
			$BLG::Notified = true;
		}
	}
};