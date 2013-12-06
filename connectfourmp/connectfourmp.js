// get reference to canvas / 2d-context
var can;
var ctx;

// gui constants
var SPACE_SIZE = 60;
var SPACE_SIZE_HALF = SPACE_SIZE / 2;
var CIRCLE_RADIUS = 25;

// game constants
var EMPTY = 0;
var PLAYER1 = 1;
var PLAYER2 = 2;

// game variables
var turnCount = 1;
var player1Wins, player2Wins, draws;
player1Wins = player2Wins = draws = 0;
var player1Turn = true;
var player2Computer = false;
var winner = -1;
var board;
var player1Color = 'red';
var player2Color = 'black';

// user variables
var username = 'Username';
var playerNumber = 0;
var playersInGame = [];
var currentGameID = '';

// network constants
var CAT_LOGIN = 1;
var CAT_GAME = 2;
var CAT_GAMEPLAY = 3;

// CAT_LOGIN IDs
var ID_LOGIN = 0;
var ID_LOGOUT = 1;
var ID_CHAT = 4;
var ID_UPDATEPLAYER = 5;

// CAT_GAME IDs
var ID_CREATEGAME = 0;
var ID_STARTGAME = 1;
var ID_ADDPLAYER = 2;
var ID_INVITEPLAYER = 3;
var ID_PLAYERINVITATION = 4;
var ID_JOINGAME = 5;
var ID_LEAVEGAME = 6;
var ID_RESETGAME = 7;

// CAT_GAMEPLAY IDs
var ID_CONNECTFOURMOVE = 100;
var ID_CONNECTFOURGAMEOVER = 101;

// network variables
var port = 7750;
var url = 'ws://chs5peteluk01.blackbaud.com:' + port;
var socket;


function initBoard() {
    var i, j;
    board = new Array(6);
    for (i = 0; i < board.length; i++) {
        board[i] = new Array(7);
        for (j = 0; j < board[i].length; j++) {
            board[i][j] = EMPTY;
        }
    }
}
function updateData() {
    var data = document.getElementById('data');
    var turn = player1Turn ? "PLAYER1" : "PLAYER2";
    data.innerHTML = "Players:<br>(red) 1: " + playersInGame[0] + " - " + player1Wins + 
							 "<br>(blk) 2: " + playersInGame[1] + " - " + player2Wins + 
							 "<br>Draws - " + draws + 
							 "<br>Turn: <b>" + turn + "</b>";
}
// graphics 'update loop' function.
function updateBoard() {
    // clear canvas
    ctx.fillStyle = "white";
    ctx.fillRect(0, 0, can.width, can.height);

    // draw board
    var boardx = 0;
    var boardy = 0;
    ctx.fillStyle = "#F0F000";
    ctx.fillRect(boardx, boardy, 7 * SPACE_SIZE, 6 * SPACE_SIZE);

    // draw empty/filled spaces
    var drawx = boardx;
    var drawy = boardy;
    var i, j;
    for (i = 0; i < board.length; i++) {
        drawx = boardx;
        drawy = i * SPACE_SIZE;
        for (j = 0; j < board[i].length; j++) {
            switch (board[i][j]) {
            case EMPTY:
                ctx.fillStyle = "white";
                break;
            case PLAYER1:
                ctx.fillStyle = player1Color;
                break;
            case PLAYER2:
                ctx.fillStyle = player2Color;
                break;
            default:
            }
            ctx.beginPath();
            ctx.arc(drawx + SPACE_SIZE_HALF, drawy + SPACE_SIZE_HALF, CIRCLE_RADIUS, 0, 360);
            ctx.fill();
            drawx += SPACE_SIZE;
        }
    }
}
function initGame() {
    turnCount = 1;
    player1Turn = true;
    winner = -1;
    initBoard();
    updateData();
    updateBoard();
}
function showInvalid() {
    var invalid = document.getElementById('invalid');
    invalid.style = "visibility:visible;font-color:red;";
    setTimeout(hideInvalid, 2000);
}
function hideInvalid() {
    var invalid = document.getElementById('invalid');
    invalid.style = "visibility:hidden;";
}
function enableControl($control) {
    $control.removeAttr('disabled');
}
function disableControl($control) {
    $control.attr('disabled','disabled');
}
function updateStatus(msg) {
    $('#pstatus').append(msg); 
}
function showOverlay() {
    $('#overlay').css('visibility','visible');
}
function hideOverlay() {
    $('#overlay').css('visibility','hidden');
}
function getLoginRequest() {
    return { "category": CAT_LOGIN, "id": ID_LOGIN, "username": "" };
}
function getLogoutRequest() {
    return { "category": CAT_LOGIN, "id": ID_LOGOUT };
}
function getChatRequest() {
    return { "category": CAT_LOGIN, "id": ID_CHAT, "text": "" };
}
function getCreateGameRequest() {
    return { "category": CAT_GAME, "id": ID_CREATEGAME, "type": "" };
}
function getStartGameRequest() {
    return { "category": CAT_GAME, "id": ID_STARTGAME, "gameID": "" };
}
function getInvitePlayerRequest() {
    return { "category": CAT_GAME, "id": ID_INVITEPLAYER, "gameID": "", "usernameInvited": "" };
}
function getJoinGameRequest() {
    return { "category": CAT_GAME, "id": ID_JOINGAME, "gameID": "" };
}
function getLeaveGameRequest() {
    return { "category": CAT_GAME, "id": ID_LEAVEGAME, "gameID": currentGameID, "playerNumber": playerNumber };
}
function getResetGameRequest() {
    return { "category": CAT_GAME, "id": ID_RESETGAME, "gameID": currentGameID };
}
function getConnectFourMoveRequest() {
    return { "category": CAT_GAMEPLAY, "id": ID_CONNECTFOURMOVE, "gameID": currentGameID, "column": 0, "playerNumber": playerNumber };
}
function send(messageObj) {
    socket.send(JSON.stringify(messageObj));
}
function leaveGame() {
	send(getLeaveGameRequest());
	
	// reset game variables
	currentGameID = '';
	playerNumber = 0;
	playersInGame = [];

	hideOverlay();
	showLobby();
}

function checkKey(e) {
    e = e || window.event;

	// login
	if (!($('#btnLogin').attr('disabled'))) {
		if (e.keyCode == '13') { //enter 13
			if($('#txtUsername').is(':focus')) { 
				$('#btnLogin').click();
			}
		}
	}
	// modal
	else if($('#overlay').is(':visible')) {
		if (e.keyCode == '27') { //esc 27
			leaveGame();
			hideOverlay();
		}		
	}
	
/*	
    if (e.keyCode == '37') { //left 37
    } else if (e.keyCode == '38') { //up 38
    } else if (e.keyCode == '39') { //right 39
    } else if (e.keyCode == '40') { //down 40
    } 
*/
}
function move(this_player, col) {
	hideInvalid();

    // returns true if valid move.
    var valid = false;
    var i;
    for (i = board.length-1; i >= 0; i--) { // move up from bottom in specific column
        if (board[i][col] == EMPTY) {
            valid = true;
            break;
        }
    }

    if (valid) {
        /*
        board[i][col] = this_player;
        updateBoard();
        */
        player1Turn = !player1Turn;
        
        // do these last.
        updateData();
        
        // check for win (4 in a row)
        var gameOver = false;
        var gameOverAlert = "";
        if (checkForWin()) {
            if (winner == PLAYER2) {
                player2Wins++;
                gameOverAlert = "Player2 wins!";
            } else {
                player1Wins++;
                gameOverAlert = "Player1 win!";
            }
            gameOver = true;
        } else if (checkForDraw()) {
            draws++;
            gameOver = true;
            gameOverAlert = "Draw!";
        }
/*
        if (gameOver) {
            updateData();
            alert(gameOverAlert);
            setTimeout(initGame, 2000); // new game in 2sec
        } else { 
            // no win or draw, game continues                
            if (!player1Turn && player2Computer) {
                setTimeout(cpuMove, 500); // schedule computer turn
            }
        }
*/
        if (socket) {
            var request = getConnectFourMoveRequest();
            request.column = col;
            send(request);
        }
        return true; //valid move.
    } else {
		//showInvalid();
	}

    return false; // invalid move.
}

function checkMouse(event) {
    var x, y;
    if (event.x) {
        x = event.x;
        y = event.y;
    } else if (event.clientX) {
        x = event.clientX;
        y = event.clientY;
    }
    event.stopPropagation(); // prevents reprocessing same mouse event

    x -= can.offsetLeft;
    y -= can.offsetTop;

    // which column did they click?
    var col = Math.floor(x / SPACE_SIZE);

    if (player1Turn && playerNumber == 1) { // if not players turn, ignore mouse input.
        move(PLAYER1, col);
    } else if (!player1Turn && playerNumber != 1) {
        move(PLAYER2, col);
	}

    return false; // prevents reprocessing same mouse event
}

function showMove(player, col) {
    var i;
    for (i = board.length-1; i >= 0; i--) { // move up from bottom in specific column
        if (board[i][col] == EMPTY) {
            break;
        }
    }
    board[i][col] = player;
    updateBoard();
}

function checkForDraw() {
    var i, j;
	for (i = 0; i < board.length; i++) {
        for (j = 0; j < board[i].length; j++) {
            if (board[i][j] == EMPTY) return false; // found an empty space, game not over.
        }
    }
    return true; // if we got here, the board is full.
}

function cpuMove(){
    var rand = document.getElementById('random');
    var ai1 = document.getElementById('ai1');

    if (rand.checked) {
        var i = Math.floor(Math.random() * 8);
        if (!move(PLAYER2, i)) { 
            setTimeout(cpuMove, 500);
        } else {
            player1Turn = true;
            updateData();
        }
    } else {
    }
}

function checkForWin() {
    // loop over rows and check for horizontal wins
    var i, j, this_player;
	for (i = 0; i < board.length; i++) {
        for (j = 0; j < 4; j++) { // 4 possible horizontal wins in each row
            this_player = board[i][j]; // get the player at the starting position of this 4-in-a-row.
            if (this_player == EMPTY) continue;
            if (board[i][j+1] == this_player
             && board[i][j+2] == this_player
             && board[i][j+3] == this_player) {
                // win.
                winner = this_player;
                return true;
            }
        }
    }
    
    // loop over columns and check for vertical wins
    for (i = 0; i < board[0].length; i++) {
        for (j = 0; j < 3; j++) { // 3 possible vertical wins in each column
            this_player = board[j][i]; // get the player at the starting position of this 4-in-a-row.
            if (this_player == EMPTY) continue;
            if (board[j+1][i] == this_player
             && board[j+2][i] == this_player
             && board[j+3][i] == this_player) {
                // win.
                winner = this_player;
                return true;
            }
        }
    }
    
    // loop over 4-block diagonal lines aiming DOWN-RIGHT (there are 12 possibilities)
    for (i = 0; i < 3; i++) {
        for (j = 0; j < 4; j++) { 
            this_player = board[i][j]; // get the player at the starting position of this 4-in-a-row.
            if (this_player == EMPTY) continue;
            if (board[i+1][j+1] == this_player
             && board[i+2][j+2] == this_player
             && board[i+3][j+3] == this_player) {
                // win.
                winner = this_player;
                return true;
            }
        }
    }
    
    // loop over 4-block diagonal lines aiming UP-RIGHT (or DOWN-LEFT) (there are 12 possibilities)
    for (i = 0; i < 3; i++) {
        for (j = 3; j < 7; j++) { 
            this_player = board[i][j]; // get the player at the starting position of this 4-in-a-row.
            if (this_player == EMPTY) continue;
            if (board[i+1][j-1] == this_player
             && board[i+2][j-2] == this_player
             && board[i+3][j-3] == this_player) {
                // win.
                winner = this_player;
                return true;
            }
        }
    }

    return false;
}

function onLoggedIn() {
    $('span#spUsername').text(username);
    hideLogin();
	$('input#btnChatSend').click(function() { 
		var request,
			$chatInput = $('input[type="text"]#txtChatInput');		
		if($chatInput.val().length > 0) {
			request = getChatRequest(),
			request.text = $chatInput.val();
			$chatInput.val('');
			send(request);
		}
	});
	showLobby();
}
function doGameCreationModal(isOwner, maxPlayers, canStart) {
    var $modal = $('#modal');
    $modal.empty();
    
    // add Player list
    var i;
	for (i = 0; i < maxPlayers; i++) {
        $modal.append('<div class="player" id="player' + (i + 1) + '">' + (i + 1) + ':</div>');
    }
    
    // add Invite controls
    $modal.append('<input type="text" id="txtInvite" placeholder="Invite User"/>');
    $modal.append('<input type="button" id="btnInvite" value="Invite"/>');
    $('#modal input#btnInvite').click( function() {
        var name = $('#modal input#txtInvite').val();
        if (name != '') {
            var request = getInvitePlayerRequest();
            request.gameID = currentGameID;
            request.usernameInvited = name;
            send(request);
        }
    });
    
    // add Start button
    if (isOwner) {
        $modal.append('<input type="button" id="btnStart" value="Start Game"/>');
        $startButton = $('#modal #btnStart');
        $startButton.click( function() {
            startGame(currentGameID);
        });
        if (!canStart) {
            disableControl( $startButton );
        }
    }
	
	// add Close button
	$modal.append('<br/><input type="button" id="btnLeave" value="Leave Game"/>');
	$('#modal #btnLeave').click( function() {
		leaveGame();
	});

    
    showOverlay();
	$('#txtInvite').focus();
}

function updateLobbyPlayerDisplay(playerID, playerDisplayName) {
	var $playerRow = $('ul#playerList > li[playerID="' + playerID + '"]');
	if($playerRow.length > 0) {
		$playerRow.text( playerDisplayName );
	} else {
		$('ul#playerList').append('<li playerID="' + playerID + '">' + playerDisplayName + '</li>');
	}
}
function removeLobbyPlayer(playerID) {
	$('ul#playerList').remove('li[playerID="' + playerID + '"]');
}
function handleMessage(event) {
    var response = $.parseJSON(event.data);
    
    if (response.category == CAT_LOGIN) {
        if (response.id == ID_LOGIN) {
            // handled in socket setup
        }
        else if (response.id == ID_LOGOUT) {
            // nothing
        }
		else if (response.id == ID_CHAT) {
			$('textarea#txtChatOutput').val( $('textarea#txtChatOutput').val() + '\n' +  response.username + ': ' + response.text);
			$('textarea#txtChatOutput').scrollTop(9999);
		}
		else if (response.id == ID_UPDATEPLAYER) {
			if(response.reason && response.reason.length > 0) {
				$('textarea#txtChatSystemOutput').val( $('textarea#txtChatSystemOutput').val() + '\n' + response.reason );
				$('textarea#txtChatSystemOutput').scrollTop(999);
			}
			if(response.status == "IN_LOBBY") {
				updateLobbyPlayerDisplay(response.playerID, response.username);
			} else if(response.status == "IN_LOBBY_CLOAKED") {
				removeLobbyPlayer(response.playerID);
			} else if (response.status == "PLAYING_GAME") {
				updateLobbyPlayerDisplay(response.playerID, response.username + ' (in game)' );
			} else if (response.status == "LOGGED_OUT") { 
				removeLobbyPlayer(response.playerID);
			}
		}
    }
    else if (response.category == CAT_GAME) {
    
        if (response.id == ID_CREATEGAME) {
            playersInGame = new Array(response.maxPlayers);
            currentGameID = response.gameID;
            doGameCreationModal(true, response.maxPlayers, response.canStartGame);
        } 
        else if (response.id == ID_STARTGAME){
            initGame();
            hideOverlay();
			hideLobby();
            $('#divBoard').show();
        } 
        else if (response.id == ID_ADDPLAYER){
            $('div#player' + response.playerNumber + '').text('' + response.playerNumber + ': ' + response.username);
            playersInGame[response.playerNumber - 1] = response.username;
            if (response.username === username) {
                playerNumber = response.playerNumber;
            }
            if (response.canStartGame) {
                enableControl( $('#btnStart') );
            }
        } 
        else if (response.id == ID_INVITEPLAYER) {
        } 
        else if (response.id == ID_PLAYERINVITATION) {
            var ok = confirm('You have been invited to join a game by ' + response.invitedByUsername);
            if (ok) {
                var request = getJoinGameRequest();
                request.gameID = response.gameID;
				currentGameID = response.gameID;
                send(request);
            }
        }
        else if (response.id == ID_JOINGAME) {
			if(response.success) {
				playersInGame = new Array(response.maxPlayers);
				doGameCreationModal(false, response.maxPlayers);
			} else {
				alert(response.reason);
				currentGameID = '';
			}
        }
        else if (response.id == ID_LEAVEGAME) {
			$('div#player' + response.playerNumber + '').text('' + response.playerNumber + ': ');
			playersInGame[response.playerNumber - 1] = null;
			alert('' + response.usernameLeftGame + ' has left the game.');
			if($('#overlay').not(':visible')) {
				leaveGame(); // only leave the game if it's in progress
			}
        }
		else if (response.id == ID_RESETGAME) {
		    initGame();
            hideOverlay();
			hideLobby();
            $('#divBoard').show();
			player1Turn = response.firstTurnPlayerNumber == 1;
			updateData();
		}
    } 
	else if (response.category == CAT_GAMEPLAY) {
    
        if (response.id == ID_CONNECTFOURMOVE) {
            if (response.success){
                showMove(response.playerNumber, response.column);
                player1Turn = response.playerNumber != 1; // if player2 moved, it's player1's turn
            } else {
                // my move failed. it's still my turn
                player1Turn = playerNumber == 1;
                alert(response.reason);
            }
			updateData();
        } else if(response.id == ID_CONNECTFOURGAMEOVER) {
			if(response.winnerPlayerNumber == playerNumber) {
				alert('You win!');
			} else {
				alert('' + (playersInGame[response.winnerPlayerNumber - 1]) + ' wins!');
			}
			if(playerNumber == 1) {
				var c = confirm('Play again?');
				if (c) { 
					send(getResetGameRequest());
				}
			}
		}
    }
}
function hideLogin() {
	$('#divLogin').hide();
}
function showLogin() {
    enableControl($('#btnLogin'));
    $('#divLogin').show();
	$('#txtUsername').focus();
}
function hideLobby() {
	$('#divLobby').hide();
}
function showLobby() {
	$('#divLobby').show();
}
function doLogout() {
    username = tempUsername = '';
    send(getLogoutRequest());
    socket.close();
    hideLobby();
}
function doCreateGame(gameName) {
    var request = getCreateGameRequest();
    request.type = gameName;
    send(request);
}
function startGame(gameID) { 
    var request = getStartGameRequest();
    request.gameID = gameID;
    send(request);
}

$(function() { 

    if (!window.WebSocket) {
        // disable page if websockets not supported
        var $error = $('#pError');    
        $error.val('WebSockets are not supported by your browser.  Please upgrade your browser to the lastest version, or switch to Google Chrome.');
        $error.css('display','block');
        $('#divLogin').hide();
        $('#divStatus').show();
    } else {
	
		hideInvalid();
        $('#txtUsername').focus();
		
        $('a#Logout').click(function() { doLogout(); });
        // wire up create-game click func's
        $('.nav > li#Games ul li a').each(function() {
            $(this).click(function() { doCreateGame($(this).id); });
        });

        $('#btnLogin').click(function() {
			$('#divStatus').hide();
            $(this).attr('disabled','disabled');
            $('.Spinner').show();

            var tempUsername = $('#txtUsername').val();

            // connect to server...
            socket = new WebSocket(url);
            socket.onopen = function(event) { 
                var loginRequest = getLoginRequest();
                loginRequest.username = tempUsername; 
                send( loginRequest );
                // loginRepsponse is handled below in socket.onmessage
            };
            socket.onclose = function(event) {
                $('.Spinner').hide();
                updateStatus('Connection to the server has been closed. ');
                showLogin();
				// user logged out.  dont show status.
				//$('#divStatus').show();
            };
            socket.onerror = function(event) {
                $('.Spinner').hide();
				$('#pstatus').text('');
                updateStatus('Error connecting to the server at: ' + event.target.URL + '  ');
                showLogin();
				$('#divStatus').show();
            };
            socket.onmessage = function(event) {
                // this is the initial onmessage handler for LoginResponse.
                $('.Spinner').hide();
                var loginResponse = $.parseJSON(event.data);
                if (loginResponse.success == 1)
                {
                    socket.onmessage = handleMessage;
                    username = tempUsername;
                    onLoggedIn();
                } else {
                    alert(loginResponse.reason);
                    $('#btnLogin').removeAttr('disabled');
                }
            };
            
        });
    }
    
    can = document.getElementById('can');
    ctx = can.getContext('2d');
    
    window.onkeydown = checkKey;
    can.addEventListener("mousedown", checkMouse, false);
});