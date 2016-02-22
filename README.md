# leagueprogram

Important:
To run the program you will need to edit the app.config file. You will need to request an api key from https://developer.riotgames.com/ to run the program.

About:
Using this program you can enter anyones ingame league of legends name and it will output there overall winrate in ranked mode as well as their winrates with anyone they queued with 4 or more times in ranked. Due to the limitations on the default riot api key this program will take 1 second per ranked game played by the user to do all the api requests the first time it is ran for a specific name. All subsequent runs on the same name should take under 10 seconds since all api responses are stored on the path you have specified in the app.config file.

