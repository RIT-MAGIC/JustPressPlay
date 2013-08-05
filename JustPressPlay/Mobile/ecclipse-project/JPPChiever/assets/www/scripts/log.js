var WRITE_MESSAGE = "";
var WRITING = false;

function readFile(theFilename, onceRead)
{
	window.requestFileSystem(
		LocalFileSystem.PERSISTENT,
		0,
		function (fileSystem) {
			fileSystem.root.getFile(
				theFilename,
				{create: true, exclusive: false},
				function (fileEntry)
				{
					fileEntry.file(
						function (file)
						{
							var reader = new FileReader();
							reader.onloadend = function(evt) {
								console.log("reading text:");
								console.log(evt.target.result);
								onceRead(evt.target.result);
							}
							reader.onerror = log_error;
							reader.readAsText(file);
						},
						log_error);
				},
				log_error);
		},
		log_error);
}

function writeLog(message)
{
	WRITE_MESSAGE += message;
	if(WRITING == false)
	{
		window.requestFileSystem(
			LocalFileSystem.PERSISTENT,
			0,
			function(fileSystem)
			{
				fileSystem.root.getFile("log.txt", 
				{create: true}, 
				function(fileEntry)
				{
					fileEntry.createWriter(
						function(writer)
						{
							writer.onwriteend = function(evt) {
								console.log("write worked");
							}
							writer.seek(writer.length);
							writer.write(WRITE_MESSAGE);
							WRITE_MESSAGE = "";
							WRITING = false;
						},
						log_error);
				}, 
				log_error);
			},
			log_error);
			
		WRITING = true;
	}
}

function writeUP(json)
{

	window.requestFileSystem(
		LocalFileSystem.PERSISTENT,
		0,
		function(fileSystem)
		{
			fileSystem.root.getFile(
			"remember.txt", 
			{create: true}, 
			function(fileEntry)
			{
				fileEntry.createWriter(
					function(writer)
					{
						writer.onwriteend = function(evt) {
							console.log("write worked");
						}
						writer.write(json);
					},
					log_error);
			}, 
			log_error);
		},
		log_error);
}

function log_error(error)
{
	alert(error);
	console.log(error.code);
}