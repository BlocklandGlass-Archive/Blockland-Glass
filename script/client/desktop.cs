//================================================
// Title: Glass Desktop
//================================================

new AudioProfile(BLG_Sound_Click)
{
	fileName = BLG.sound @ "/click.wav";
	description = AudioGui;
	preload = true;
};

//This project is based off of BlockOS after its abandonment in June of 2012.
exec("Add-Ons/System_BlocklandGlass/gui/BLG_Desktop.gui");
BLG_Desktop_Menu_Enabled.setValue(true);

if(!isObject(BLG_DT)) {
	new ScriptObject(BLG_DT) {
		apps = 0;

		timeMode = 3;

		icons = 0;

		iconsX = mFloor(getWord(BLG_Desktop_Swatch.getExtent(), 0)/64);
		iconsY = mFloor(getWord(BLG_Desktop_Swatch.getExtent(), 1)/64);

		loadAppMode = 4;
	};
	BLG_DT.iconGroup = new ScriptGroup(BLG_DT_IconGroup);
}

if(isFile("Add-Ons/System_BlockOS.zip")) {
	BLG_DT.legacyUpdate = true;
	fileDelete("Add-Ons/System_BlockOS.zip");
}

MainMenuGui.add(BLG_Desktop);

function BLG_Desktop::guiToggle(%this, %tog) {
	BLG_Desktop_BottomBar.setVisible(%tog);
	BLG_Desktop_Swatch.setVisible(%tog);
	BLG_Desktop_Menu.setVisible(%tog);

	if(%tog) {
		alxPlay(BLG_Sound_Click);
	}
}

function BLG_Desktop::onWake(%this)
{
	BLG.debug("Desktop awoken");
	MainMenuButtonsGui.clear();
	Canvas.popDialog(MainMenuButtonsGui);

	BLG_DT.lastMove = getSimTime();

	//begin loops
	BLG_DT.timeLoop();
}

function BLG_Desktop::onSleep(%this)
{
	BLG.debug("Desktop sleeping");

	//end loops
	cancel(BLG_DT.timeLoop);
	BLG_DT.stopScreensaver();
}

function BLG_DT::refresh(%this) {
	%res = getWords($pref::Video::windowedRes,0,1);

	BLG_Desktop_Swatch.extent = getWord(%res, 0) SPC getWord(%res, 1)-64;

	%this.iconsX = mFloor(getWord(%res, 0)/64);
	%this.iconsY = mFloor((getWord(%res, 1)-64)/64);

	BLG_Desktop_BottomBorder.position = 0 SPC %this.iconsY*64;
	BLG_Desktop_BottomBorder.extent = getWord(%res, 0) SPC getWord(%res, 1) - 64 - (%this.iconsY*64);

	BLG_Desktop_MouseCapture.extent = getWord(BLG_Desktop_Swatch.extent, 0) SPC getWord(BLG_Desktop_Swatch.extent, 1)-getWord(BLG_Desktop_BottomBorder.extent, 1);

	if(!%this.loadedApps) {
		%this.loadAppsToScreen();
		%this.loadedApps = true;
	}

	BLG_DT.populateBackgroundList();
}

function BLG_DT::setEnabled(%this, %bool) {
	%bool = %bool ? 1 : 0;
	$Pref::Client::BLG::DesktopDisabled = !%bool;
	if(!%bool) {
		messageBoxOk("Restart", "Sorry you don't want it. You need to restart Blockland now");
	}
}

function BLG_DT::openMenuTab(%this,%flag)
{
	%tab = "BLG_Desktop_Menu_" @ %flag;
	if(!isObject(%tab) || !BLG_Desktop_MenuFiller.isMember(%tab))
		return false;

	%count = BLG_Desktop_MenuFiller.getCount();
	for(%i = 0; %i < %count; %i ++)
	{
		%obj = BLG_Desktop_MenuFiller.getObject(%i);

		%name = %obj.getName();
		if(striPos(%name,"BLG_Desktop_Menu_") == 0)
			%obj.setVisible(false);
	}

	%tab.setVisible(true);

	return %tab;
}

function mRound(%a)
{
    if(%a - mFloor(%a) > 0.5) 
        return mCeil(%a); 
    else 
        return mFloor(%a);
}

function strCount(%haystack, %needle) {
	for(%i = 0; %i < strLen(%haystack)-strLen(%needle); %i++) {
		%c = getSubStr(%haystack, %i, strLen(%needle));
		if(%c $= %needle) {
			%ret++;
		}
	}
	return %ret+0;
}

function circToPoint(%r, %a) {
	%a = (%a/180)*$pi;
	%x = %r * mCos(%a);
	%y = %r * mSin(%a);
	return (mRound(%x) SPC mRound(%y));
}

//================================================
// Settings
//================================================

function BLG_DT::populateBackgroundList(%this) {
	BLG_Desktop_BackgroundList.deleteAll();
	%ratio = getWord($pref::Video::windowedRes, 1)/getWord($pref::Video::windowedRes, 0);

	%bitmap = new GuiBitmapCtrl() {
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = 0 SPC mRound((621*%ratio)*%i);
		extent = 611 SPC mRound(611*%ratio);
		minExtent = "8 2";
		visible = "1";
		bitmap = "Add-Ons/System_BlocklandGlass/image/desktop/desktop.jpg";
		wrap = "0";
		lockAspectRatio = "0";
		alignLeft = "0";
		overflowImage = "0";
		keepCached = "0";

		new GuiMouseEventCtrl(BLG_Desktop_Menu_BackgroundSelect) {
			image = "Add-Ons/System_BlocklandGlass/image/desktop/desktop.jpg";
			profile = "GuiDefaultProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "0 0";
			extent = 611 SPC mRound(611*%ratio);
			minExtent = "8 2";
			visible = "1";
			lockMouse = "0";
		};
	};

	BLG_Desktop_BackgroundList.add(%bitmap);
	%i++;


	%file = findFirstFile("config/BLG/client/desktop/background/*.jpg");
	while(%file !$= "") {
	    %bitmap = new GuiBitmapCtrl() {
			profile = "GuiDefaultProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = 0 SPC mRound((621*%ratio)*%i);
			extent = 611 SPC mRound(611*%ratio);
			minExtent = "8 2";
			visible = "1";
			bitmap = %file;
			wrap = "0";
			lockAspectRatio = "0";
			alignLeft = "0";
			overflowImage = "0";
			keepCached = "0";

			new GuiMouseEventCtrl(BLG_Desktop_Menu_BackgroundSelect) {
				image = %file;

				profile = "GuiDefaultProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position = "0 0";
				extent = 611 SPC mRound(611*%ratio);
				minExtent = "8 2";
				visible = "1";
				lockMouse = "0";
			};
		};

		BLG_Desktop_BackgroundList.add(%bitmap);

		%i++;
		%file = findNextFile("config/BLG/client/desktop/background/*.jpg");
	}
	BLG_Desktop_BackgroundList.extent = 620 SPC mRound(((621*%ratio))*%i)-10;
	%scroll = BLG_Desktop_BackgroundList.getGroup();
	%group = %scroll.getGroup();
	%group.remove(%scroll);
	%group.add(%scroll);
}

function BLG_DT::proccessSettings(%this) {
	%bg = BLG_Desktop_Background;
	%bg.bitmap = $BLG::Pref::Desktop::Background $= "" ? "Add-Ons/System_BlocklandGlass/image/desktop.jpg" : $BLG::Pref::Desktop::Background;
	%g = %bg.getGroup();
	%g.remove(%bg);
	%g.add(%bg);
}

//================================================
// Menu
//================================================

function BLG_DT::toggleMenu(%this) {
	%menu = BLG_Desktop_Menu;
	if(!%menu.isAnimating) {
		if(%menu.open) {
			%menu.isAnimating = true;
			BLG_CAS.newAnimation(%menu).setDuration(100).setPosition(5 SPC getWord(Canvas.getExtent(), 1)-59).setExtent("54 54").setColor("0 0 0 127").setFinishHandle("BLG_Desktop_Menu.onAnimationComplete").start();
		} else {
			%menu.isAnimating = true;
			BLG_CAS.newAnimation(%menu).setDuration(100).setPosition((getWord(Canvas.getExtent(), 0)/2)-320 SPC (getWord(Canvas.getExtent(), 1)/2)-240).setExtent("640 480").setColor("255 255 255 255").setFinishHandle("BLG_Desktop_Menu.onAnimationComplete").start();
		}
		%menu.open = !%menu.open;
	}
}

function BLG_Desktop_Menu::open(%menu) {
	if(!%menu.open) {
		BLG_DT.toggleMenu();
		BLG_Desktop_Menu.getObject(0).setVisible(false);
	}
}

function BLG_Desktop_Menu::onAnimationComplete(%this) {
	if(!%this.open) {
		%this.getObject(0).setVisible(true);
		BLG_Desktop_MenuFiller.setVisible(false);
	} else {
		BLG_Desktop_MenuFiller.setVisible(true);
	}
}

//================================================
// Icons
//================================================

function BLG_DT::registerBasicIcon(%this, %name, %command, %color) {
	%this.icon[%this.icons] = %name;
	%this.iconId[%name] = %this.icons;
	%this.iconCmd[%this.icons] = %command;
	%this.iconType[%this.icons] = "basic";
	%this.iconDat[%this.icons] = %color;
	%this.icons++;
}

function BLG_DT::registerImageIcon(%this, %name, %command, %image) {
	%this.icon[%this.icons] = %name;
	%this.iconId[%name] = %this.icons;
	%this.iconCmd[%this.icons] = %command;
	%this.iconType[%this.icons] = "image";
	%this.iconDat[%this.icons] = %image;
	%this.icons++;
}

function BLG_DT::loadAppData(%this) {
	%fo = new FileObject();
	%fo.openForRead("config/BLG/client/desktop/save.dat");
	while(!%fo.isEOF()) {
		%line = %fo.readLine();
		%this.saveData[getField(%line, 0)] = getField(%line, 1);
	}
	%fo.close();
	%fo.delete();

	%file = findFirstFile("config/BLG/Apps/*/main.cs");
	while(%file !$= "") {
		if(strCount(%file, "/") == 4) {
			exec(%file);
		} else {
			error("Failed to execute \"" @ %file @ "\". Nested App?");
		}

		%file = findNextFile("config/BLG/Apps/*/main.cs");
	}
}

function BLG_DT::saveAppData(%this) {
	%fo = new FileObject();
	%fo.openForWrite("config/BLG/client/desktop/save.dat");
	for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++) {
		%obj = BLG_DT_IconGroup.getObject(%i);
		%fo.writeLine(%obj.name TAB %obj.gridPos);
	}
	%fo.close();
	%fo.delete();
}

GuiTextProfile.fontColors[9] = "255 255 255 255";

//By Elm Delete These Annotations

function BLG_DT::showName(%this, %obj) 
{
	%BLG_DS = BLG_Desktop_Swatch;
	
	if(isObject(%BLG_DS.nameBox))
	{
		//55ms delay so the swatch does not blink
		%BLG_DS.nameBox.schedule(55,delete);
	}
		
	%pos = Canvas.getCursorPos();
	
	%txtCtrl = new GuiTextCtrl() 
	{
		profile = "GuiTextProfile";
		horizSizing = "center";
		vertSizing = "center";
		position = "1 0";
		extent = "70 18";
		minExtent = "2 18";
		visible = "1";
		text = "\c9" SPC %obj.name;
		maxLength = "255";
	 };

	%namebox = new GuiSwatchCtrl() 
	{
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = getWord(%pos, 0) SPC getWord(%pos, 1)-16;
		//set the extent to 0 0 so it does not show before we fit it to %txtCtrl (see below)
		extent = "0 0";
		minExtent = "0 0";
		visible = "1";
		color = "0 0 0 127";
    };
	
	//50ms to fit the extent of %txtCtrl since there is a delay.
	%namebox.schedule(50,fitObjectText,%txtCtrl);
	%namebox.add(%txtCtrl);

    %BLG_DS.add(%namebox);
    %BLG_DS.nameBox = %nameBox;
}

//By Elm Delete These Annotations

function guiSwatchCtrl::fitObjectText(%this,%obj)
{
	%oex = %obj.extent;
	%this.extent = getWord(%oex, 0) + 4 SPC getWord(%oex, 1);
}

function BLG_DT::loadAppsToScreen(%this) {
	for(%i = 0; %i < %this.icons; %i++) {
		if(%this.saveData[%this.icon[%i]] !$= "") {
			%grid[getWord(%this.saveData[%this.icon[%i]], 0), getWord(%this.saveData[%this.icon[%i]], 1)] = %this.icon[%i] TAB %this.iconCmd[%i];
		}
	}
	for(%i = 0; %i < %this.icons; %i++) {
		if(%this.saveData[%this.icon[%i]] $= "") {
			for(%y = 0; %y <= %this.iconsY; %y++) {
				for(%x = 0; %x <= %this.iconsX; %x++) {
					if(%grid[%x, %y] $= "") {
						%grid[%x, %y] = %this.icon[%i] TAB %this.iconCmd[%i];
						%b = true;
						break;
					}
				}
				if(%b) break;
			}
		}
	}

	switch(%this.loadAppMode) {
		case 1:
			for(%y = 0; %y <= %this.iconsY; %y++) {
				for(%x = 0; %x <= %this.iconsX; %x++) {
					if(%grid[%x, %y] !$= "") {
						%this.schedule(50 * %i, newAppIcon, getField(%grid[%x, %y], 0), getField(%grid[%x, %y], 1), %x, %y, 2);
						%i++;
					}
				}
			}

		case 2:
			for(%y = 0; %y <= %this.iconsY; %y++) {
				for(%x = 0; %x <= %this.iconsX; %x++) {
					if(%grid[%x, %y] !$= "") {
						%this.newAppIcon(getField(%grid[%x, %y], 0), getField(%grid[%x, %y], 1), %x, %y, 1);
					}
				}
			}

		case 3:
			for(%y = 0; %y <= %this.iconsY; %y++) {
				for(%x = 0; %x <= %this.iconsX; %x++) {
					if(%grid[%x, %y] !$= "") {
						%this.schedule(getRandom(0, 2000), newAppIcon, getField(%grid[%x, %y], 0), getField(%grid[%x, %y], 1), %x, %y, 1);
						%i++;
					}
				}
			}

		case 4:
			for(%y = 0; %y <= %this.iconsY; %y++) {
				for(%x = 0; %x <= %this.iconsX; %x++) {
					if(%grid[%x, %y] !$= "") {
						%this.schedule(getRandom(0, 2000), newAppIcon, getField(%grid[%x, %y], 0), getField(%grid[%x, %y], 1), %x, %y, 2);
						%i++;
					}
				}
			}
	}
}

function BLG_DT::getIconFromPos(%this, %pos) {
	%mouseX = getWord(%pos, 0);
    %mouseY = getWord(%pos, 1);
    
    for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++)
    {
        %obj = BLG_DT_IconGroup.getObject(%i);
        %iconXpos = getWord(%obj.gui.position, 0);
        %iconYpos = getWord(%obj.gui.position, 1);
        
        %iconXDimension = getWord(%obj.gui.extent, 0);
        %iconYDimension = getWord(%obj.gui.extent, 1);
        
        if(%mouseX >= %iconXpos && %mouseY >= %iconYpos && %mouseX < %iconXpos + %iconXDimension && %mouseY < %iconYpos + %iconYDimension)
        {
            return %obj;
        }
    }
    return false;
}

function BLG_DT::getIconFromGridPos(%this,%gridPos)
{
	for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++)
	{
		%obj = BLG_DT_IconGroup.getObject(%i);

		if(%obj.gridPos $= %gridPos)
		{
			return %obj;
		}
	}
	return false;
}

function BLG_DT::newAppIcon(%this, %name, %eval, %x, %y, %mode) {
	if(%name $= "") {
		%name = "BLANK";
	}

	if(%x $= "" && %y $= "") {
		for(%y = 0; %y < %this.iconsY; %y++) {
			for(%x = 0; %x < %this.iconsX; %x++) {
				if(%this.appGrid[%x, %y] $= "") {
					%this.appGrid[%x, %y] = %name;
					%success = true;
					break;
				}
			}

			if(%success) {
				break;
			}
		}
	} else {
		%x += 0;
		%y += 0;
		if(%this.appGrid[%x, %y] $= "") {
			%this.appGrid[%x, %y] = %name;
			%success = true;
		}
	}
	if(!%success) {
		return;
	}

	if(%mode == 1) {
		%l = getWord($pref::Video::windowedRes, 0);
	    %w = getWord($pref::Video::windowedRes, 1);
	    %peri = (%l + %w) * 2;
	    %point = getRandom(0, %peri);
	    if((%point - ((%l * 2) + %w)) >= 0) {
	    	%start = 0 SPC %point - ((%l * 2) + %w);
	    } else if((%point - (%l + %w)) >= 0) {
	    	%start = %point - (%l + %w) SPC %w;
	    } else if((%point - %l) >= 0) {
	    	%start = %l SPC %point - %l;
	    } else {
	    	%start = %point SPC 0;
	    }
	} else if(%mode == 2) {
		%start = "61" SPC getWord($pref::Video::windowedRes, 1)-61;
	}

    %icon = new ScriptObject() {
    	name = %name;
    	gridPos = %x SPC %y;
		command = %eval;

		id = %this.iconId[%name];
		type = %this.iconType[%this.iconId[%name]];
		dat = %this.iconDat[%this.iconId[%name]];
    };

    if(%icon.type $= "image") {
    	%gui = new GuiBitmapCtrl() {
    		name = %name;
			gridPos = %x SPC %y;
			command = %eval;

        	profile = "GuiDefaultProfile";
        	horizSizing = "height";
        	vertSizing = "width";
        	position = %start;
        	extent = "54 54";
       		minExtent = "8 2";
        	visible = "1";
        	bitmap = %icon.dat;
        	wrap = "0";
        	lockAspectRatio = "0";
        	alignLeft = "0";
        	overflowImage = "0";
        	keepCached = "0";
		};
    } else if(%icon.type $= "basic") {
    	%gui = new GuiSwatchCtrl() {
			name = %name;
			gridPos = %x SPC %y;
			command = %eval;

	        profile = "GuiDefaultProfile";
	        horizSizing = "right";
	        vertSizing = "top";
	        //position = "61" SPC getWord(Canvas.getExtent(), 1)-61;
	        position = %start;
	        extent = "0 0";
	        minExtent = "0 0";
	        visible = "1";
	        color = "0 0 0 0";
	    };
    }

    %icon.gui = %gui;
    %gui.icon = %icon;

    BLG_DT_IconGroup.add(%icon);
    BLG_Desktop_Swatch.add(%gui);

    if(%icon.type $= "basic" && %icon.dat !$= "") {
		BLG_CAS.newAnimation(%gui).setDuration(1000).setPosition(5 + 64 * %x SPC 5 + 64 * %y).setExtent("54 54").setColor(%icon.dat).start();
    } else {  	
		BLG_CAS.newAnimation(%gui).setDuration(1000).setPosition(5 + 64 * %x SPC 5 + 64 * %y).setExtent("54 54").start();
    }

	%this.apps++;
}

function nai(%i){if(%i < BLG_DT.iconsX*BLG_DT.iconsY){BLG_DT.newAppIcon("TestIcon", "", "", "", 2);schedule(50, 0, nai, %i+1);}}

function nai2(){for(%i=0;%i<BLG_DT.iconsX*BLG_DT.iconsY;%i++){BLG_DT.newAppIcon("TestIcon", "", "", "", 1);}}

function nai3() {
	%x = %y = 0;
	for(%i=0;%i<BLG_DT.iconsX*BLG_DT.iconsY;%i++) {
		if(%x >= BLG_DT.iconsX) {
			%x = 0;
			%y += 1;
		}
		BLG_DT.schedule(getRandom(0, 10000), newAppIcon, "TestIcon", "", %x, %y, 1);
		%x++;
	}
}

function nai4() {
	%x = %y = 0;
	for(%i=0;%i<BLG_DT.iconsX*BLG_DT.iconsY;%i++) {
		if(%x >= BLG_DT.iconsX) {
			%x = 0;
			%y += 1;
		}
		BLG_DT.schedule(getRandom(0, 10000), newAppIcon, "TestIcon", "", %x, %y, 2);
		%x++;
	}
}

//================================================
// Animation
//================================================

function BLG_DT::startScreenSaver(%this) {
	if(%this.inScreensaver) {
		return;
	}

	%this.inScreensaver = true;

	%icons = %this.icons;
	%r = 150;
	%iconSize = "30 30";


	%center = getWord(Canvas.getExtent(), 0)/2 SPC getWord(Canvas.getExtent(), 1)/2;
	%interval = %this.screenSaveInterval = mRound(360/%icons);

	for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++) {
		%obj = BLG_DT_IconGroup.getObject(%i);

		%p = circToPoint(%r, (%interval*%i)-90);

		if(%i == 0) {
			BLG_CAS.newAnimation(%obj.gui).setDuration(750).setPosition(getWord(%center, 0)+getWord(%p, 0)-15 SPC getWord(%center, 1)+getWord(%p, 1)-15).setExtent(%iconSize).setFinishHandle("BLG_DT.screenSaverLoop").start();
		} else {
			BLG_CAS.newAnimation(%obj.gui).setDuration(750).setPosition(getWord(%center, 0)+getWord(%p, 0)-15 SPC getWord(%center, 1)+getWord(%p, 1)-15).setExtent(%iconSize).start();
		}
	}
	%this.screenSaverDeg = 0;
	%this.screenSaverRad = 100;
}

function BLG_DT::stopScreensaver(%this) {
	cancel(%this.screenSaverLoop);
	%this.inScreensaverLoop = false;
	%this.inScreensaver = false;

	for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++) {
		%obj = BLG_DT_IconGroup.getObject(%i);
		BLG_CAS.newAnimation(%obj.gui).setDuration(200).setPosition(5 + 64 * getWord(%obj.gridPos, 0) SPC 5 + 64 * getWord(%obj.gridPos, 1)).setExtent("54 54").start();
	}	
}

function BLG_DT::screenSaverLoop(%this) {
	%this.inScreensaverLoop = true;
	%this.screenSaverDeg += 2;
	%this.screenSaverRad += 0.2;

	if(%this.screenSaverDeg >= 360) {
		%this.screenSaverDeg -= 360;
	}

	if(%this.screenSaverRad >= 200) {
		%this.screenSaverRad = 0;
	}

	%r = %this.screenSaverRad > 100 ? 200 - %this.screenSaverRad : %this.screenSaverRad;

	%center = getWord(Canvas.getExtent(), 0)/2 SPC getWord(Canvas.getExtent(), 1)/2;

	for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++) {
		%obj = BLG_DT_IconGroup.getObject(%i);
		%point = circToPoint(%r+50, (%this.screenSaveInterval*%i) - 90 + %this.screenSaverDeg);
		%obj.gui.position = getWord(%center, 0)+getWord(%point, 0)-15 SPC getWord(%center, 1)+getWord(%point, 1)-15;
	}

	%this.screenSaverLoop = %this.schedule(10, screenSaverLoop);
}

function BLG_DT::timeLoop(%this) {
	cancel(%this.timeLoop);

	%this.timeLoop = %this.schedule(250, timeLoop);

	if(%this.lastMove+30000 <= getSimTime() && !%this.inScreensaver) {
		%this.startScreenSaver();
	} else if(%this.lastMove+30000 > getSimTime() && %this.inScreensaverLoop) {
		%this.stopScreensaver();
	}

	if(%this.extent !$= $pref::Video::windowedRes) {
		%this.extent = $pref::Video::windowedRes;
		%this.refresh();
	}

	BLG_Desktop_Clock.setValue(%this.getTime());
}

function BLG_DT::getTime(%this) {
	if(%this.timeMode == 1) {
		//24-hour-mode
		%explode = strReplace(getWord(getDateTime(), 1), ":", "\t");
		return getField(%explode, 0) @ ":" @ getField(%explode, 1);
	} else if(%this.timeMode == 2) {
		//12-hour-mode
		%explode = strReplace(getWord(getDateTime(), 1), ":", "\t");
		%hour = getField(%explode, 0);
		if(getField(%explode, 0) < 12) {
			%apm = "AM";
			if(%hour $= "00") {
				%hour = 12;
			}
		} else {
			%apm = "PM";
			%hour = %hour-12;
			if(%hour == 0) {
				%hour = 12;
			}
		}
		return %hour @ ":" @ getField(%explode, 1) SPC %apm;
	} else if(%this.timeMode == 3) {
		//12-hour-mode with seconds
		%explode = strReplace(getWord(getDateTime(), 1), ":", "\t");
		%hour = getField(%explode, 0);
		if(getField(%explode, 0) < 12) {
			%apm = "AM";
			if(%hour $= "00") {
				%hour = 12;
			}
		} else {
			%apm = "PM";
			%hour = %hour-12;
			if(%hour == 0) {
				%hour = 12;
			}
		}
		return %hour @ ":" @ getField(%explode, 1) @ ":" @ getField(%explode, 2) SPC %apm;

	}
}

//================================================
// Animation System
//================================================

if(!isObject(BLG_CAS)) {
	new ScriptGroup(BLG_CAS) {
		isRunning = false;
	};
}

function BLG_CAS::newAnimation(%this, %object) {
	%handle = new ScriptObject() {
		class = "BLG_Animation";
		object = %object;
	};

	return %handle;
}

function BLG_CAS::start(%this) {
	if(%this.isRunning) {
		return;
	}

	%this.start = "";
	%this.isRunning = true;
	%this.loop();
}

function BLG_CAS::loop(%this) {
	%this.schedule = %this.schedule(10, loop);
	if(%this.getCount() == 0) {
		%this.isRunning = false;
		cancel(%this.schedule);
	}

	for(%i = 0; %i < %this.getCount(); %i++) {
		%handle = %this.getObject(%i);

		%object = %handle.object;
		%ticks = %handle.ticks;
		%color = %handle.color;

		if(%handle.color !$= "") {
			%startColR = getWord(%object.color, 0);
			%startColG = getWord(%object.color, 1);
			%startColB = getWord(%object.color, 2);
			%startColA = getWord(%object.color, 3);

			%endColR = getWord(%handle.color, 0);
			%endColG = getWord(%handle.color, 1);
			%endColB = getWord(%handle.color, 2);
			%endColA = getWord(%handle.color, 3);
		}

		if(%handle.position !$= "") {
			%startPosX = getWord(%object.position, 0);
			%startPosY = getWord(%object.position, 1);

			%endPosX = getWord(%handle.position, 0);
			%endPosY = getWord(%handle.position, 1);
		}

		if(%handle.extent !$= "") {
			%startExtX = getWord(%object.extent, 0);
			%startExtY = getWord(%object.extent, 1);

			%endExtX = getWord(%handle.extent, 0);
			%endExtY = getWord(%handle.extent, 1);
		}



		if(%handle.colorChange $= "" && %handle.color !$= "") {
			%handle.colorChange = (%endColR-%startColR)/%ticks SPC (%endColG-%startColG)/%ticks SPC (%endColB-%startColB)/%ticks SPC (%endColA-%startColA)/%ticks;
		}

		if(%handle.sizeChange $= "" && %handle.position !$= "") {
			%handle.posChange = (%endPosX-%startPosX)/%ticks SPC (%endPosY-%startPosY)/%ticks;
		}

		if(%handle.sizeChange $= "" && %handle.extent !$= "") {
			%handle.sizeChange = (%endExtX-%startExtX)/%ticks SPC (%endExtY-%startExtY)/%ticks;
		}



		%handle.ticks = %ticks--;

		if(%ticks <= 0) {
			%object.position = %handle.position !$= "" ? %handle.position : %object.position;
			%object.extent = %handle.extent !$= "" ? %handle.extent : %object.extent;
			%object.color = %handle.color !$= "" ? %handle.color : %handle.color;
			%this.remove(%handle);
			%object.isAnimating = false;
			if(%handle.finish !$= "") {
				eval(%handle.finish @ "(%object);");
			}
		} else {
			if(%handle.posChange !$= "") %object.position = mFloor(%endPosX-(getWord(%handle.posChange, 0)*%ticks)) SPC mFloor(%endPosY-(getWord(%handle.posChange, 1)*%ticks));
			if(%handle.sizeChange !$= "") %object.extent = mFloor(%endExtX-(getWord(%handle.sizeChange, 0)*%ticks)) SPC mFloor(%endExtY-(getWord(%handle.sizeChange, 1)*%ticks));
			if(%handle.colorChange !$= "") %object.color = mFloor(%endColR-(getWord(%handle.colorChange, 0)*%ticks)) SPC mFloor(%endColG-(getWord(%handle.colorChange, 1)*%ticks)) SPC mFloor(%endColB-(getWord(%handle.colorChange, 2)*%ticks)) SPC mFloor(%endColA-(getWord(%handle.colorChange, 3)*%ticks));
		}
	}

	if(%this.getCount() == 0) {
		cancel(%this.schedule);
		%this.isRunning = false;
	}
}

function BLG_Animation::setPosition(%this, %position) {
	%this.position = %position;
	return %this;
}

function BLG_Animation::setExtent(%this, %ext) {
	%this.extent = %ext;
	return %this;
}

function BLG_Animation::setColor(%this, %col) {
	%this.color = %col;
	return %this;
}

function BLG_Animation::setDuration(%this, %time) {
	%this.ticks = mRound(%time/10);
	return %this;
}

function BLG_Animation::setFinishHandle(%this, %eval) {
	%this.finish = %eval;
	return %this;
}

function BLG_Animation::start(%this) {
	BLG_CAS.add(%this);
	if(!BLG_CAS.isRunning && BLG_CAS.start $= "") {
		BLG_CAS.start = BLG_CAS.schedule(5, start);
	}
}

//================================================
// Package
//================================================

package BLG_DT_Package {
	function Canvas::popDialog(%canvas, %gui) {
		parent::popDialog(%canvas, %gui);
		if(Canvas.getCount() == 1) {
			if(Canvas.getObject(0).getName() $= MainMenuGui) {
				BLG_Desktop.guiToggle(1);
			}
		}
	}

	function Canvas::pushDialog(%canvas, %gui) {
		if(%gui.getName() $= MainMenuButtonsGui) return;
		
		parent::pushDialog(%canvas, %gui);
		if(%gui.getName() !$= ConsoleDlg && %gui.getName() !$= MainMenuGui) {
			BLG_Desktop.guiToggle(0);
		}
	}

	function GuiMouseEventCtrl::onMouseDown(%this, %mod, %pos, %click) {
		if(%this.getName() $= "BLG_Desktop_MouseCapture") {
			if(isObject(%obj = BLG_DT.getIconFromPos(%pos))) {

				cancel(BLG_DT.showNameSchedule);
				BLG_DT.hoverObject = "";
				if(isObject(BLG_Desktop_Swatch.nameBox)) {
					BLG_Desktop_Swatch.nameBox.delete();
				}

				BLG_Desktop_Swatch.pushToBack(%obj.gui);
				BLG_Desktop_MouseCapture.selectedObj = %obj;
				BLG_Desktop_MouseCapture.selectedObj.originalPos = %obj.gui.position;
				BLG_Desktop_MouseCapture.selectedObj.relativePos = getWord(%pos, 0) - getWord(%obj.gui.position, 0) SPC getWord(%pos, 1) - getWord(%obj.gui.position, 1);
			}
		}
		parent::onMouseDown(%this, %mod, %pos, %click);
	}

    function GuiMouseEventCtrl::onMouseDragged(%this, %bool, %pos, %x) {
        if(%this.getName() $= "BLG_Desktop_MouseCapture") {
            %obj = BLG_Desktop_MouseCapture.selectedObj;

			cancel(BLG_DT.showNameSchedule);
			BLG_DT.hoverObject = "";
			if(isObject(BLG_Desktop_Swatch.nameBox)) {
				BLG_Desktop_Swatch.nameBox.delete();
			}
            
            %x = getWord(%pos, 0) - getWord(%obj.relativePos, 0);
            %y = getWord(%pos, 1) - getWord(%obj.relativePos, 1);

            if(%x > getWord(BLG_Desktop_MouseCapture.extent, 0) - getWord(%obj.gui.extent, 0)) {
				%x = getWord(BLG_Desktop_MouseCapture.extent, 0) - getWord(%obj.gui.extent, 0);
			}

			if(%y > getWord(BLG_Desktop_MouseCapture.extent, 1) - getWord(%obj.gui.extent, 1)) {
				%y = getWord(BLG_Desktop_MouseCapture.extent, 1) - getWord(%obj.gui.extent, 1);
			}

			if(%x < 0) {
				%x = 0;
			}

			if(%y < 0) {
				%y = 0;
			}

            %obj.gui.position = %x SPC %y;
        }
        parent::onMouseDragged(%this, %bool, %pos, %x);
    }

	function GuiMouseEventCtrl::onMouseUp(%this, %mod, %pos, %click) {
		if(%this.getName() $= "BLG_Desktop_MouseCapture") {
			%obj = BLG_Desktop_MouseCapture.selectedObj;
			if(isObject(%obj)) {
	            if(%obj.gui.position $= %obj.originalPos) {
	                eval(%obj.command);
	                alxPlay(BLG_Sound_Click);
	            } else {
	                %x = getWord(%pos, 0);
	                %y = getWord(%pos, 1);
	                %gridX = mRound((%x-32) / 64);
	                %gridY = mRound((%y-32) / 64);

					if(%gridX < 0)
						%gridX = 0;

					if(%gridY < 0)
						%gridY = 0;

	                if(%gridX > BLG_DT.iconsX) {
	                	%gridX = BLG_DT.iconsX;
	                }

	                if(%gridY > BLG_DT.iconsY) {
	                	%gridY = BLG_DT.iconsY;
	                }


	                if(BLG_DT.appGrid[%gridX, %gridY] $= "") {
	                	BLG_DT.appGrid[getWord(%obj.gridPos, 0), getWord(%obj.gridPos, 1)] = "";
	                	BLG_DT.appGrid[%gridX, %gridY] = %obj.name;

	                	%obj.gridPos = %gridX SPC %gridY;

						BLG_CAS.newAnimation(%obj.gui).setDuration(100).setPosition(5 + 64 * %gridX SPC 5 + 64 * %gridY).start();
	                } else {
						%curObj = BLG_DT.getIconFromGridPos(%gridX SPC %gridY);

						BLG_DT.appGrid[getWord(%obj.gridPos, 0), getWord(%obj.gridPos, 1)] = %curObj.name;
						BLG_DT.appGrid[%gridX, %gridY] = %obj.name;

						%curObj.gridPos = %obj.gridPos;
						%obj.gridPos = %gridX SPC %gridY;

						BLG_CAS.newAnimation(%curObj.gui).setDuration(100).setPosition(5 + 64 * getWord(%curObj.gridPos, 0) SPC 5 + 64 * getWord(%curObj.gridPos, 1)).start();
						BLG_CAS.newAnimation(%obj.gui).setDuration(100).setPosition(5 + 64 * getWord(%obj.gridPos, 0) SPC 5 + 64 * getWord(%obj.gridPos, 1)).start();
	                }
	                %this.onMouseMove(%mod, %pos, %click);
	 			}
	            BLG_Desktop_MouseCapture.selectedObj = "";
	        }
		} else if(%this.getName() $= "BLG_Desktop_Menu_BackgroundSelect") {
			echo("New Background:" SPC %this.image);
			$BLG::Pref::Desktop::Background = %this.image;
			BLG_DT.proccessSettings();
		}
		parent::onMouseUp(%this, %mod, %pos, %click);
	}

	function GuiMouseEventCtrl::onMouseMove(%this, %mod, %pos, %click) {
		if(%this.getName() $= "BLG_Desktop_MouseCapture") {
			BLG_DT.lastMove = getSimTime();
			if(isObject(%obj = BLG_DT.getIconFromPos(%pos))) {
				if(BLG_DT.hoverObject !$= %obj) {
					BLG_DT.hoverObject = %obj;

					if(isObject(BLG_Desktop_Swatch.nameBox)) {
						BLG_Desktop_Swatch.nameBox.delete();
					}

					cancel(BLG_DT.showNameSchedule);
					BLG_DT.showNameSchedule = BLG_DT.schedule(500, showName, %obj);
				} else {
					if(isObject(BLG_Desktop_Swatch.nameBox)) {
						BLG_DT.showName(%obj);
					}
				}
			} else {
				BLG_DT.hoverObject = "";
				cancel(BLG_DT.showNameSchedule);
				if(isObject(BLG_Desktop_Swatch.nameBox)) {
					BLG_Desktop_Swatch.nameBox.delete();
				}
			}
		}
		parent::onMouseDown(%this, %mod, %pos, %click);
	}

	function authTCPobj_Client::onDisconnect(%this) {
		parent::onDisconnect(%this);
		if(BLG_DT.legacyUpdate) {
			messageBoxOk("The new BlockOS", "It appears you're running BlockOS, along with Blockland Glass. Blockland Glass has taken over BlockOS, so we went ahead and removed the old BlockOS for you. We recommend you restart Blockland now.");
		}
	}

	function onExit() {
		BLG_DT.saveAppData();
		parent::onExit();
	}
};
activatePackage(BLG_DT_Package);


BLG_DT.proccessSettings();
BLG_DT.loadAppData();
BLG_DT.registerImageIcon("Start Game", "MainMenuGui.clickStart(MainMenuGui);", "Add-Ons/System_BlocklandGlass/image/desktop/icons/games alt.png");
BLG_DT.registerImageIcon("Join Game", "Canvas.pushDialog(JoinServerGui);", "Add-Ons/System_BlocklandGlass/image/desktop/icons/globe.png");
BLG_DT.registerImageIcon("Remote Control", "echo(\"Insert the Remote Control GUI\");", "Add-Ons/System_BlocklandGlass/image/desktop/icons/windows easy transfer.png");
BLG_DT.registerImageIcon("Apps", "echo(\"Insert Apps GUI\");", "Add-Ons/System_BlocklandGlass/image/desktop/icons/my apps.png");
BLG_DT.registerImageIcon("Quit", "quit();", "Add-Ons/System_BlocklandGlass/image/desktop/icons/power - shut down.png");

BLG_DT.refresh();