//================================================
// Title: Glass Gui Client 
//================================================

if(!isObject(BLG_GDS)) {
	new ScriptObject(BLG_GDS) {
		sg = "";
	};

	new ScriptGroup(BLG_Objects) {
		guis = 0;
	};

	BLG_GDS.sg = BLG_Objects;
}

function BLG_GDS::registerObject(%this, %obj) {
	if(isObject(%obj)) {
		if(!BLG_Objects.isMember(%obj)) {

			%so = new ScriptObject() {
				class = BLG_GuiObject;

				baseObj = %obj;
				id = BLG_Objects.objs;
				children = "";
			};

			BLG_Objects.obj[BLG_Objects.objs] = %obj;
			BLG_Objects.add(%so);
			BLG_Objects.objs++;

			%so.getAttributes();
			return %so;
		}
	}
}

function BLG_GuiObject::getAttributes(%this) {
	if(isObject(%this.baseObj)) {
		%obj = %this.baseObj;
		%file = "config/BLG/server/temp/gui.txt";
		if(isFile(%file)) {
			fileDelete(%file);
		}
		%obj.save(%file);

		%fo = new FileObject();
		%fo.openForRead(%file);
		while(!%fo.isEOF()) {
			%line = trim(%fo.readLine());
			if(strPos(%line, "new ") == 0) {
				if(%started) {
					break;
				} else {
					%started = true;
				}
			} else if(strPos(%line, "};") == 0) {
				break;
			} else if(%line $= "") {
				continue;
			} else if(%started) {
				echo("Line: [" @ %line @ "]");
				%data = getSubStr(%line, 0, strPos(%line, " = "));
				%value = getSubStr(%line, strPos(%line, " = ")+3, strLen(%line));
				%value = strReplace(%value, "\\\"", "[[quotation]]");
				%value = strReplace(%value, "\"", "");
				%value = strReplace(%value, "[[quotation]]", "\\\"");
				%value = getSubStr(%value, 0, strLen(%value)-1);

				if(%data $= "command" || %data $= "altCommand" || %data $= "closeCommand") {
					continue;
				}

				%this.attributeData[%this.attributes] = %data;
				%this.attributeValue[%this.attributes] = %value;
				%this.attributes++;
			}
		}
		%fo.close();
		%fo.delete();
		fileDelete(%file);
	}
}

function BLG_GuiObject::transfer(%this, %client) {
	%time = 0;
	%delay = 5;

	%this.schedule(%time+=%delay, send, "0" TAB %this.id TAB %this.baseObj.getClassName() TAB %this.baseObj.getName() TAB ((%this.baseObj.children $= "") ? 0 : 1));
	%this.schedule(%time+=%delay, send, "1" TAB %this.id TAB "command" TAB %this.specialValue["command"]);
	%this.schedule(%time+=%delay, send, "1" TAB %this.id TAB "altCommand" TAB %this.specialValue["altCommand"]);
	%this.schedule(%time+=%delay, send, "1" TAB %this.id TAB "closeCommand" TAB %this.specialValue["closeCommand"]);

	for(%i = 0; %i < %this.attributes; %i++) {
		%this.schedule(%time+=%delay, send, "1" TAB %this.id TAB %this.attributeData[%i] TAB %this.attributeValue[%i]);
	}

	for(%i = 0; %i < getWordCount(%this.children); %i++) {
		%childId = getWord(%this.children, %i);
		%this.schedule(%time+=%delay, send, "2" TAB %this.id TAB %childId);
	}
}

function BLG_GuiObject::send(%this, %client, %msg) {
	commandtoclient(%client, 'BLG_ObjectInfo', %msg);
}

function BLG_GuiObject::updateAttribute(%this, %client, %data, %value) {

}