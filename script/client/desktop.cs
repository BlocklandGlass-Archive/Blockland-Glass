//================================================
// Title: Glass Desktop
//================================================

//This project is based off of BlockOS after its abandonment in June of 2012.

if(!isObject(BLG_DT)) {
	new ScriptObject(BLG_DT) {
		apps = 0;
		animations = 0;
		timeMode = 3;
	};
}

MainMenuGui.add(BLG_Desktop);

function BLG_DT::populateApps(%this) {

}

//================================================
// Menu
//================================================

function BLG_DT::toggleMenu(%this) {
	%menu = BLG_Desktop_Menu;
	if(!%menu.isAnimating) {
		if(%menu.open) {
			%menu.isAnimating = true;
			%this.animation[%this.animations] = %menu TAB 10 TAB 10 SPC getWord(Canvas.getExtent(), 1)-74 TAB "64 64" TAB "0 0 0 127";
			%this.animations++;
		} else {
			%menu.isAnimating = true;
			%this.animation[%this.animations] = %menu TAB 10 TAB (getWord(Canvas.getExtent(), 0)/2)-320 SPC (getWord(Canvas.getExtent(), 1)/2)-240 TAB "640 480" TAB "255 255 255 255";
			%this.animations++;
		}
		%menu.open = !%menu.open;
	}
}

function BLG_DT::newAppIcon(%this) {
	%gui = new GuiSwatchCtrl() {
         profile = "GuiDefaultProfile";
         horizSizing = "right";
         vertSizing = "top";
         position = "15" SPC getWord(Canvas.getExtent(), 1)-74;
         extent = "0 0";
         minExtent = "0 0";
         visible = "1";
         color = "0 0 0 0";
    };
    BLG_Desktop_Swatch.add(%gui);

	%this.animation[%this.animations] = %gui TAB 100 TAB 6+70*(%this.apps) SPC 6 TAB "64 64" TAB "0 0 0 255";
	%this.animations++;


	%this.apps++;
}

function BLG_Desktop_Menu::open(%menu) {
	if(!%menu.open) {
		BLG_DT.toggleMenu();
		BLG_Desktop_Menu.getObject(0).setVisible(false);
	}
}

function BLG_Desktop_Menu::onAnimationComplete(%this) {

}

//================================================
// Animation
//================================================

function BLG_DT::animate(%this) {
	cancel(%this.animateLoop);

	BLG_Desktop_Clock.setValue(%this.getTime());

	%this.animateLoop = %this.schedule(10, animate);
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
			%object.onAnimationComplete();
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
	function GuiMouseEventCtrl::onMouseUp(%this, %mod, %pos, %click) {
		%extent = Canvas.getExtent();
		if(%this.getName() $= "BLG_Desktop_MouseCapture") {
			if(getWord(%pos, 0) < 74 && getWord(%pos, 0) > 10) {
				if(getWord(%pos, 1) < getWord(%extent, 1)-10 && getWord(%pos, 1) > getWord(%extent, 1)-74) {
					if(!BLG_Desktop_Menu.open) {
						BLG_DT.toggleMenu();
					}
				}
			}
		}
		parent::onMouseUp(%this, %mod, %pos, %click);
	}
};
activatePackage(BLG_DT_Package);

BLG_DT.animate();