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
				id = BLG_Objects.guis;
				children = "";
			};

			BLG_Objects.gui[BLG_Objects.guis] = %obj;
			BLG_Objects.add(%so);
			BLG_Objects.guis++;

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

				%this.attributeData[%this.attributes] = %data;
				%this.attributeValue[%this.attributes] = %value;
				echo("Data: [" @ %data @ "]");
				echo("Value: [" @ %value @ "]");
				%this.attributes++;
			}
		}
		%fo.close();
		%fo.delete();
		fileDelete(%file);
	}
}

function BLG_GuiObject::transfer(%this, %client) {
	commandtoclient(%client, 'BLG_ObjectInfo', "0" TAB %this.id TAB %this.baseObj.getClassName() TAB %this.baseObj.getName() TAB ((%this.baseObj.children $= "") ? 0 : 1));
	commandtoclient(%client, 'BLG_ObjectInfo', "1" TAB %this.id TAB "command");
	
}

function BLG_GuiObject::updateAttribute(%this, %client, %data, %value) {

}