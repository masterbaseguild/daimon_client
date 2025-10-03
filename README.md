# Daimon Client

This is the codebase for the Daimon Client, developed in Unity and not yet published.
Codebase del Daimon Client, sviluppato in Unity e non ancora pubblicato.
Il Daimon Client fa parte del Daimon Engine, un game engine basato su Unity con voxel engine e multiplayer studiato per supportare user generated content.

## Features

- Fetching di media dinamici da bucket MinIO hostato autonomamente
- Fetching di dati dinamici da REST API hostata autonomamente (Daimon Backend)
- Interfaccia di connessione con username autogenerato e indicatore di progresso
- Protocollo di pacchetti personalizzato ibrido, con supporto per UDP e TCP, e sistema di connessione a doppio handshake per entrambi i protocolli
- Voxel Engine a doppia griglia, con supporto per blocchi pieni e mezzi blocchi, e doppio materiale per blocchi trasparenti e opachi
- Character controller con gravità, movimento WASD, salto, gradi di velocità, volo e phasing
- (Vedi Branch versary_demo) Edit mode, con modifica libera della mappa da parte dei giocatori, e interfaccia per selezione libera dei blocchi
- (WIP) Sistema di modelli personalizzati, per ulteriore personalizzazione della mappa
- Recupero dinamico dalla API e dal bucket delle risorse relative ai blocchi in base a quanto definito nel dizionario ricevuto dal server
- Gestione della posizione dei giocatori, con pacchetti di sincronizzazione
- (WIP) Play mode, con blocco della modifica della mappa, e sistema di abilità e oggetti
- (WIP) Keepalive del client, con quit automatico in caso di timeout
- (WIP) Interfaccia C#-Lua per scripting di abilità e oggetti

## Installazione

- Clonare la repository tramite `git clone <repository-url>`
- Aprire il progetto in Unity (versione 6000.0.42f1)
- Compilare ed eseguire la build tramite `File -> Build and Run`
- Nella schermata di connessione, inserire l'indirizzo IP del server a cui si vuole effettuare la connessione