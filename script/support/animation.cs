//================================================
// Animation System
//================================================


function mRound(%a)
{
    if(%a - mFloor(%a) > 0.5) 
        return mCeil(%a); 
    else 
        return mFloor(%a);
}

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