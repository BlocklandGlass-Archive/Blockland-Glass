function newQueue(%handle) {
	return new ScriptObject() {
		class = "QueueObject";
		key = 0;
		processKey = 0;
		handle = %handle;
	};
}

function QueueObject::addItem(%this, %identifier) {
	%item = new ScriptObject() {
		class = "QueueItem";

		identifier = %identifier;
		key = %this.key;
	};
	%this.item[%this.key] = %item;
	%this.key++;
	%this.schedule(0, internal_processQueue);
	return %item.key;
}

function QueueObject::removeItem(%this, %key) {
	%this.item[%key] = -1;
	return true;
}

function QueueObject::internal_processQueue(%this) {
	if(%this.processing) {
		return;
	}

	if(%this.item[%this.processKey] $= "") {
		return;
	}

	%this.processing = true;
	eval(%this.handle @ "(" @ %this.processKey @ ", \"" @ %this.item[%this.processKey].identifier @ "\");");
}

function QueueObject::reportFinished(%this, %itemKey) {
	if(%itemKey != %this.processKey) {
		return -1;
	}

	%item = %this.item[%itemKey];

	if(%item.finished) {
		return -1;
	}

	%item.finished = true;

	%this.processKey++;
	%this.processing = false;
	%this.internal_processQueue();
}