//================================================
// Title: Glass Desktop
//================================================

//This project is based off of BlockOS after its abandonment in June of 2012.

if(!isObject(BLG_DT)) {
	new ScriptObject(BLG_DT) {
		apps = 0;

		animations = 0;
		timeMode = 2;

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

function BLG_DT::refresh(%this) {
	if(!%this.loadedApps) {
		%this.loadAppsToScreen();
		%this.loadedApps = true;
	}

	%w = (getWord(BLG_Desktop_BottomBar.getExtent(), 0) - 25)/4;
	BLG_Desktop_Join.position = "5 5";
	BLG_Desktop_Host.position = 10 + %w SPC 5;
	BLG_Desktop_Settings.position = 15 + %w*2 SPC 5;
	BLG_Desktop_Quit.position = 20 + %w*3 SPC 5;

	BLG_Desktop_Join.extent = %w SPC 59;
	BLG_Desktop_Host.extent = %w SPC 59;
	BLG_Desktop_Settings.extent = %w SPC 59;
	BLG_Desktop_Quit.extent = %w SPC 59;

	BLG_Desktop_MouseCapture.extent = BLG_Desktop_Swatch.extent = getWord(Canvas.getExtent(), 0) SPC getWord(Canvas.getExtent(), 1)-64;
	%this.iconsX = mFloor(getWord(BLG_Desktop_Swatch.getExtent(), 0)/64);
	%this.iconsY = mFloor(getWord(BLG_Desktop_Swatch.getExtent(), 1)/64);
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
// Menu
//================================================

function BLG_DT::toggleMenu(%this) {
	%menu = BLG_Desktop_Menu;
	if(!%menu.isAnimating) {
		if(%menu.open) {
			%menu.isAnimating = true;
			%this.animation[%this.animations] = %menu TAB 10 TAB 5 SPC getWord(Canvas.getExtent(), 1)-59 TAB "54 54" TAB "0 0 0 127";
			%this.animations++;
		} else {
			%menu.isAnimating = true;
			%this.animation[%this.animations] = %menu TAB 10 TAB (getWord(Canvas.getExtent(), 0)/2)-320 SPC (getWord(Canvas.getExtent(), 1)/2)-240 TAB "640 480" TAB "255 255 255 255";
			%this.animations++;
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

function BLG_DT::showName(%this, %obj) {
	if(isObject(BLG_Desktop_Swatch.nameBox))
		BLG_Desktop_Swatch.nameBox.delete();
	%pos = Canvas.getCursorPos();

	%namebox = new GuiSwatchCtrl() {
		profile = "GuiDefaultProfile";
		horizSizing = "width";
		vertSizing = "height";
		position = getWord(%pos, 0) SPC getWord(%pos, 1)-16;
		extent = "75 18";
		minExtent = "8 2";
		visible = "1";
		color = "0 0 0 127";

        new GuiTextCtrl() {
            profile = "GuiTextProfile";
            horizSizing = "center";
            vertSizing = "center";
            position = "2 0";
            extent = "70 18";
            minExtent = "70 18";
            visible = "1";
            text = "\c9" SPC %obj.name;
            maxLength = "255";
         };
    };
    BLG_Desktop_Swatch.add(%namebox);
    BLG_Desktop_Swatch.nameBox = %nameBox;
}

function BLG_DT::loadAppsToScreen(%this) {
	for(%i = 0; %i < %this.icons; %i++) {
		if(%this.saveData[%this.icon[%i]] !$= "") {
			%grid[getWord(%this.saveData[%this.icon[%i]], 0), getWord(%this.saveData[%this.icon[%i]], 1)] = %this.icon[%i] TAB %this.iconCmd[%i];
		}
	}
	for(%i = 0; %i < %this.icons; %i++) {
		if(%this.saveData[%this.icon[%i]] $= "") {
			for(%y = 0; %y < %this.iconsY; %y++) {
				for(%x = 0; %x < %this.iconsX; %x++) {
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
			for(%y = 0; %y < %this.iconsY; %y++) {
				for(%x = 0; %x < %this.iconsX; %x++) {
					if(%grid[%x, %y] !$= "") {
						%this.schedule(50 * %i, newAppIcon, getField(%grid[%x, %y], 0), getField(%grid[%x, %y], 1), %x, %y, 2);
						%i++;
					}
				}
			}

		case 2:
			for(%y = 0; %y < %this.iconsY; %y++) {
				for(%x = 0; %x < %this.iconsX; %x++) {
					if(%grid[%x, %y] !$= "") {
						%this.newAppIcon(getField(%grid[%x, %y], 0), getField(%grid[%x, %y], 1), %x, %y, 1);
					}
				}
			}

		case 3:
			for(%y = 0; %y < %this.iconsY; %y++) {
				for(%x = 0; %x < %this.iconsX; %x++) {
					if(%grid[%x, %y] !$= "") {
						%this.schedule(getRandom(0, 2000), newAppIcon, getField(%grid[%x, %y], 0), getField(%grid[%x, %y], 1), %x, %y, 1);
						%i++;
					}
				}
			}

		case 4:
			for(%y = 0; %y < %this.iconsY; %y++) {
				for(%x = 0; %x < %this.iconsX; %x++) {
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
		%l = getWord(Canvas.getExtent(), 0);
	    %w = getWord(Canvas.getExtent(), 1);
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
		%start = "61" SPC getWord(Canvas.getExtent(), 1)-61;
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

    %color = "0 0 0 0";
    if(%icon.type $= "basic" && %icon.dat !$= "") {
    	%color = %icon.dat;
    }


	%this.animation[%this.animations] = %gui TAB 100 TAB 5 + 64 * %x SPC 5 + 64 * %y TAB "54 54" TAB %color;
	%this.animations++;


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
	%r = 100;
	%iconSize = "30 30";


	%center = getWord(Canvas.getExtent(), 0)/2 SPC getWord(Canvas.getExtent(), 1)/2;
	%interval = %this.screenSaveInterval = mRound(360/%icons);

	for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++) {
		%obj = BLG_DT_IconGroup.getObject(%i);

		%p = circToPoint(%r, (%interval*%i)-90);
		echo(%p);

		%this.animation[%this.animations] = %obj.gui TAB 20 TAB getWord(%center, 0)+getWord(%p, 0)-15 SPC getWord(%center, 1)+getWord(%p, 1)-15 TAB %iconSize TAB (%obj.dat !$= "" ? %obj.dat : "255 255 255 255");
		%this.animations++;
	}
	%this.screenSaverRad = 100;
	%this.schedule(200, screenSaverLoop);
}

function BLG_DT::stopScreensaver(%this) {
	%this.inScreensaverLoop = false;
	%this.inScreensaver = false;

	for(%i = 0; %i < BLG_DT_IconGroup.getCount(); %i++) {
		%obj = BLG_DT_IconGroup.getObject(%i);

		%this.animation[%this.animations] = %obj.gui TAB 20 TAB 5 + 64 * getWord(%obj.gridPos, 0) SPC 5 + 64 * getWord(%obj.gridPos, 1) TAB "54 54" TAB %obj.gui.color;
		%this.animations++;
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
}

function BLG_DT::animate(%this) {
	cancel(%this.animateLoop);

	%this.animateLoop = %this.schedule(10, animate);

	if(%this.lastMove+30000 <= getSimTime() && !%this.inScreensaver) {
		%this.startScreenSaver();
	} else if(%this.lastMove+30000 > getSimTime() && %this.inScreensaverLoop) {
		%this.stopScreensaver();
	}

	if(%this.inScreensaverLoop) {
		%this.screenSaverLoop();
	}

	if(%this.extent !$= Canvas.getExtent()) {
		%this.extent = Canvas.getExtent();
		%this.refresh();
	}

	BLG_Desktop_Clock.setValue(%this.getTime());
	for(%i = 0; %i < %this.animations; %i++) {
		%data = %this.animation[%i];
		%object = getField(%data, 0);
		%ticks = getField(%data, 1);
		%color = getField(%data, 4);
		%startPosX = getWord(%object.position, 0);
		%startPosY = getWord(%object.position, 1);
		%startExtX = getWord(%object.extent, 0);
		%startExtY = getWord(%object.extent, 1);
		%startColR = getWord(%object.color, 0);
		%startColG = getWord(%object.color, 1);
		%startColB = getWord(%object.color, 2);
		%startColA = getWord(%object.color, 3);
		%endPosX = getWord(getField(%data, 2), 0);
		%endPosY = getWord(getField(%data, 2), 1);
		%endExtX = getWord(getField(%data, 3), 0);
		%endExtY = getWord(getField(%data, 3), 1);
		%endColR = getWord(getField(%data, 4), 0);
		%endColG = getWord(getField(%data, 4), 1);
		%endColB = getWord(getField(%data, 4), 2);
		%endColA = getWord(getField(%data, 4), 3);

		if(%this.animationStart[%i] $= "") {
			%object.isAnimating = true;
			%this.animationStart[%i] = %object.position SPC %object.extent;
			%this.animationChange[%i] = (%endPosX-%startPosX)/%ticks SPC (%endPosY-%startPosY)/%ticks SPC (%endExtX-%startExtX)/%ticks SPC (%endExtY-%startExtY)/%ticks;
			%this.animationColor[%i] = (%endColR-%startColR)/%ticks SPC (%endColG-%startColG)/%ticks SPC (%endColB-%startColB)/%ticks SPC (%endColA-%startColA)/%ticks;
		}

		%ticks--;
		%this.animation[%i] = %object TAB %ticks TAB getField(%data, 2) TAB getField(%data, 3) TAB getField(%data, 4);

		if(%ticks == 0) {
			%object.position = getField(%data, 2);
			%object.extent = getField(%data, 3);
			%object.color = getField(%data, 4);
			%this.animation[%i] = "";
			%this.animationStart[%i] = "";
			%this.animationChange[%i] = "";
			%this.animationColor[%i] = "";
			%object.isAnimating = false;
			if(isFunction(%object, onAnimationComplete)) {
				%object.onAnimationComplete();
			}
		} else {
			%object.position = mFloor(%endPosX-(getWord(%this.animationChange[%i], 0)*%ticks)) SPC mFloor(%endPosY-(getWord(%this.animationChange[%i], 1)*%ticks));
			%object.extent = mFloor(%endExtX-(getWord(%this.animationChange[%i], 2)*%ticks)) SPC mFloor(%endExtY-(getWord(%this.animationChange[%i], 3)*%ticks));
			%object.color = mFloor(%endColR-(getWord(%this.animationColor[%i], 0)*%ticks)) SPC mFloor(%endColG-(getWord(%this.animationColor[%i], 1)*%ticks)) SPC mFloor(%endColB-(getWord(%this.animationColor[%i], 2)*%ticks)) SPC mFloor(%endColA-(getWord(%this.animationColor[%i], 3)*%ticks));
		}
	}

	%lastFound = -1;
	for(%i = 0; %i < %this.animations; %i++) {
		if(%this.animation[%i] $= "" && %this.animation[%i+1] !$= "") {
			%this.animation[%i] = %this.animation[%i+1];
			%this.animation[%i+1] = "";
			%this.animationStart[%i+1] = "";
			%this.animationChange[%i+1] = "";
			%this.animationColor[%i+1] = "";
			%lastFound = %i;
		} else if(%this.animation[%i] !$= "") {
			%lastFound = %i;
		}
	}
	%this.animations = %lastFound+1;
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
		} else {
			%apm = "PM";
			%hour = %hour-12;
		}
		return %hour @ ":" @ getField(%explode, 1) SPC %apm;
	} else if(%this.timeMode == 3) {
		//12-hour-mode with seconds
		%explode = strReplace(getWord(getDateTime(), 1), ":", "\t");
		%hour = getField(%explode, 0);
		if(getField(%explode, 0) < 12) {
			%apm = "AM";
		} else {
			%apm = "PM";
			%hour = %hour-12;
		}
		return %hour @ ":" @ getField(%explode, 1) @ ":" @ getField(%explode, 2) SPC %apm;

	}
}

//================================================
// Package
//================================================

package BLG_DT_Package {
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

            if(%x > getWord(BLG_Desktop_Swatch.extent, 0) - getWord(%obj.gui.extent, 0)) {
				%x = getWord(BLG_Desktop_Swatch.extent, 0) - getWord(%obj.gui.extent, 0);
			}

			if(%y > getWord(BLG_Desktop_Swatch.extent, 1) - getWord(%obj.gui.extent, 1)) {
				%y = getWord(BLG_Desktop_Swatch.extent, 1) - getWord(%obj.gui.extent, 1);
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

	                	BLG_DT.animation[BLG_DT.animations] = %obj.gui TAB 10 TAB 5 + 64 * %gridX SPC 5 + 64 * %gridY TAB "54 54" TAB %obj.gui.color;
						BLG_DT.animations++;
	                } else {
	                	BLG_DT.animation[BLG_DT.animations] = %obj.gui TAB 10 TAB 5 + 64 * getWord(%obj.gridPos, 0) SPC 5 + 64 * getWord(%obj.gridPos, 1) TAB "54 54" TAB %obj.gui.color;
						BLG_DT.animations++;
	                }
	                %this.onMouseMove(%mod, %pos, %click);
	 			}
	            BLG_Desktop_MouseCapture.selectedObj = "";
	        }
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
			messageBoxOk("The new BlockOS", "It appears you're running BlockOS, along with Blockland Glass. Blockland Glass has taken over BlockOS, so we went ahead and removed BlockOS for you. We recommend you restart Blockland now.");
		}
	}

	function onExit() {
		BLG_DT.saveAppData();
		parent::onExit();
	}
};
activatePackage(BLG_DT_Package);


BLG_DT.loadAppData();
BLG_DT.registerBasicIcon("Join Server", "Canvas.pushDialog(JoinServerGui);", "0 0 255 255");
BLG_DT.registerBasicIcon("Host Server", "Canvas.pushDialog(startMissionGui);", "0 255 0 255");
BLG_DT.registerBasicIcon("Settings", "Canvas.pushDialog(OptionsDlg);", "127 127 127 255");
BLG_DT.registerBasicIcon("Quit", "quit();", "255 0 0 255");
BLG_DT.registerImageIcon("BLG", "BLG_ToggleOverlay(0);", "Add-Ons/System_BlocklandGlass/image/logo.jpg");

BLG_DT.animate();
BLG_DT.refresh();