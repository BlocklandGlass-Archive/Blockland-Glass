//================================================
// Title: Glass Notification System
//================================================

if(!isObject(BLG_GNS)) {
	new ScriptObject(BLG_GNS) {
		queue = newQueue("BLG_GNS.queue");
	};
}

new AudioProfile(BLG_Sound_Notification)
{
	fileName = BLG.sound @ "/noti.wav";
	description = AudioGui;
	preload = true;
};

function BLG_GNS::queue(%this, %id, %ident) {
	echo(%id TAB %ident);
	%action = getField(%ident, 0);
	%obj = getField(%ident, 1);

	switch$(%action) {
		case "add":
			%obj.currentQueueId = %id;
			%obj.finalize();

		case "remove":
			%obj.currentQueueId = %id;
			%obj.pop();
	}
}

function BLG_GNS::newNotification(%this, %title, %text, %icon, %time, %key, %sound, %command, %closeOnOverlay) {
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
		sound = %sound;

		overlay = %closeOnOverlay;
	};

	%this.key[%key] = %so;

	%so.draw();
	return %so;
}

function BLG_GNS::updateNotification(%this, %key, %title, %text, %icon, %time) {
	if(%time $= "") {
    	%time = 5000;
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
	%this.key[%key].sched = BLG_GNS.queue.schedule(%time, addItem, "remove" TAB %this.key[%key]);
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
			command = "BLG_GNS.queue.addItem(\"remove TAB" @ %this @ "\");" @ %this.command;
		};
	};
	%this.gui = %gui;
	if(Canvas.getContent().getName() $= "PlayGui")
		PlayGui.add(%gui);
	else
		MainMenuGui.add(%gui);
	
	BLG_GNS.queue.schedule(32, addItem, "add" TAB %this);
}

function BLG_Notification::finalize(%this) {
	echo("ADD");
	%this.action = "add";
	%gui = %this.gui;

	%res = getRes();
	%msg = %gui.getObject(2).extent;

	%gui.getObject(3).extent = %gui.extent = "200" SPC 26+getWord(%msg, 1);
	%gui.obj = %this;

	BLG_GNS.slot[BLG_GNS.notifications++] = %gui;
	BLG_GNS.height += 31+getWord(%msg, 1);
	%this.id = BLG_GNS.notifications;

	%gui.position = getWord(%gui.position, 0) SPC getWord(%res, 1)-(BLG_GNS.height);

	BLG_CAS.newAnimation(%gui).setPosition(getWord(%res, 0)-205 SPC getWord(%res, 1) - (BLG_GNS.height)).setDuration(250).setColor(%gui.color).setFinishHandle(%this @ ".actionFinished").start();
	if(%this.sound) alxPlay(BLG_Sound_Notification);
}

function BLG_Notification::pop(%this) {
	%this.key[%this.key] = "";
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
		//BLG_GNS.schedule(100, processQueue, %this.tid);
		%qid = %this.currentQueueId;
		%this.currentQueueId = "";
		BLG_GNS.queue.schedule(100, reportFinished, %qid);
		return;
	} else if(%this.action $= "add") {
		if(%this.time > -1) {
			%this.sched = BLG_GNS.queue.schedule(%this.time, addItem, "remove" TAB %this);
		}
	} else {
		return;
	}

	%qid = %this.currentQueueId;
	%this.currentQueueId = "";
	BLG_GNS.queue.reportFinished(%qid);
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
				alxPlay(BLG_Sound_Notification);
				return;
			}
		}
		BLG_GNS.newNotification(%title, %message, %icon, %holdTime, %key, false, "BLG.getOverlay().fadeIn();", true);
	}

	function RTB_Overlay::onWake(%this) {
		parent::onWake(%this);
		for(%i = 0; %i < BLG_GNS.notifications; %i++) {
			if(BLG_GNS.slot[%i+1].obj.overlay) {
				BLG_GNS.queue.addItem("remove" TAB BLG_GNS.slot[%i+1].obj);
			}
		}
	}

	function MM_AuthBar::blinkSuccess(%this) {
		parent::blinkSuccess(%this);
		if(!$BLG::Notified) {
			BLG_GNS.newNotification("Welcome to BLG!", "Welcome to Blockland Glass! Click on me to access BLG, or simply press \"shift tab\".", "", 0, "", false, "BLG.getOverlay().fadeIn();", true);
			$BLG::Notified = true;
		}
	}
};