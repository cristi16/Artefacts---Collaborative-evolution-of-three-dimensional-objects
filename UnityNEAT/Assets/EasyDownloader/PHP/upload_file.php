<?php

	if (!empty($_FILES) && !empty($_POST) && !empty($_POST["path"]))
	{
        
               if ($_FILES["file"]["error"] > 0)
               { 
                     echo "Return Code: " . $_FILES["file"]["error"] . ""; 
               } 
               else  
                     { 
			// Desired folder structure
			$structure = getcwd() . "/" . $_POST["path"];

			if (!file_exists($structure))
                        {	
				// To create the nested structure, the $recursive parameter 
				// to mkdir() must be specified.
	
				if (!mkdir($structure, 0777, true)) {
					    echo "Unable to create directory " . $structure;
				}
			}
                        
                         if (file_exists($structure . "/" . $_FILES["file"]["name"]))
                         {
                                 echo $_FILES["file"]["name"] . " already exists. ";
                         }
                          else
                          {
               			move_uploaded_file($_FILES["file"]["tmp_name"], $structure . "/" . $_FILES["file"]["name"]);
                               echo "Uploaded to " . $structure . "/" . $_FILES["file"]["name"];
                          }
                     }
	}
	else
	{
		echo "No file or path to save.";
	}
         
?>