import React, {Component, ReactElement} from 'react';
import {Select} from "antd";
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {Logger, LoggerModule} from "../utility/logger";

const {Option} = Select;

enum EntryType {
  None,
  NpcDescription,
  CorpseDescription,
  System,
  Battle,
}

const defaultFilters = [ 'System', EntryType[EntryType.NpcDescription], EntryType[EntryType.CorpseDescription] ];
const filterKeys = [ 'System', EntryType[EntryType.NpcDescription], EntryType[EntryType.CorpseDescription] ];
let filterSelections: ReactElement[] = [];

function generateFilterSelections() {
  filterSelections = [];
  filterKeys.forEach(key => {
    filterSelections.push(
      <Option key={filterSelections.length} value={key}>{key}</Option>
    );
  });
}

function updateFilterSelections(key: string) {
  if(filterKeys.indexOf(key) === -1) {
    filterKeys.push(key);
    generateFilterSelections();
  }
}

generateFilterSelections();

type Entry = { time: string, char: string, content: string, type: EntryType };
type NPC = {name: string, title: string};

interface IProps {
}

interface IState {
  loading: boolean,
  data?: [],
  connection?: HubConnection
  selected: string[], 
  players?: string[], 
  npcs?: NPC[]
}

export class RuntimeJourney extends Component<IProps, IState> {
  static displayName = RuntimeJourney.name;

  signalrUrl = 'https://localhost:7200/hubs/chat';
  apiUrlBase = 'https://localhost:44455';

  // @ts-ignore
  constructor(props) {
    super(props);
    this.state = {loading: true, selected: defaultFilters}
  }

  componentDidMount() {
    this.setState({selected: defaultFilters});
    // this.populateJourneys();

    const connection = new HubConnectionBuilder()
      .withUrl(this.signalrUrl)
      .withAutomaticReconnect()
      .build();
    
    if(connection) {
      this.setState({connection: connection});

      connection.start()
        .then(result => {
          Logger.Log(LoggerModule.SignalR, `Connected.`);

          // Register the method which is called by Hub in server: `Clients.All.ReceiveMessage(message)`.
          connection.on('ReceiveMessage', message => {
            Logger.Log(LoggerModule.SignalR, `Receive message.\n${message.user}:${message.message}.`);
          });

          this.startBroadcast();
        })
        .catch(e => Logger.Log(LoggerModule.SignalR, `Connect failed. ${e}.`));
    }
  }
  
  componentWillUnmount() {
    Logger.Log(LoggerModule.SignalR, "RuntimeJourney will unmont.");
    // this.stopBroadcast();
  }

  handleFiltersChange(value: string[]) {
    console.log('handleChange', value);
    this.setState({selected: value});
  }

  handlePlayerSelectionChange(value: string) {

  }

  handleNpcSelectionChange(value: string) {

  }

  isNPC(char: string, content: string): boolean {
    const patterns = [`${char} the `, `${char} of the `, `${char} from `];
    for (let i = 0; i < patterns.length; i++) {
      if (content.split(' ')[0] !== char) {
        continue;
      }

      const pattern = patterns[i];
      if (content.includes(pattern)) {
        return true;
      }
    }
    return false;
  }

  isCorpse(content: string): boolean {
    const splits = content.split(' ');
    if(splits.length < 2 || (splits[0] !== 'a' && splits[0] !== 'an')) {
      return  false;
    }

    let result = false;
    for (let i = 2; i < splits.length; i++) {
      if (splits[i] === 'corpse') {
        result = true;
        break;
      }
    }

    return result;
  }

  renderJourney(journeys: any[]) {
    return (
      journeys.map((j, jIndex) =>
        <div key={`journey_${jIndex}`}>
          {(j as []).map((entry: Entry, eIndex) =>
            <div key={`entry_${jIndex}_${eIndex}`}> {`${entry.time} ${entry.char}: ${entry.content}`} </div>
          )}
        </div>))
  }

  render() {
    const journeys = this.state.data;

    if(!journeys) {
      return <div></div>;
    }

    let selected = this.state.selected;
    const filteredJourneys: Entry[][] = [];

    journeys.forEach((journey: Entry[]) => {
      filteredJourneys.push(
        journey.filter((entry: Entry) => {
          return selected.indexOf(entry.char) === -1
            && selected.indexOf(EntryType[entry.type]) === -1
            && entry.content != entry.char;
        })
      );
    })
    console.log('journeys', filteredJourneys)

    // @ts-ignore
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      // @ts-ignore
      : this.renderJourney(filteredJourneys);

    // console.log(this.state)

    return (
      <div>
        <h1 id="journey">UO Journey</h1>

        {/*Players*/}
        <Select defaultValue={this.state.players && this.state.players[0]} style={{ width: 220 }} onChange={this.handlePlayerSelectionChange.bind(this)}>
          {
            this.state.players && this.state.players.length > 0
              ? this.state.players.map((player, idx) => {
                return <Option value={player} key={idx}>{player}</Option>
              })
              : []
          }
        </Select>

        {/*NPCs*/}
        <Select defaultValue={this.state.npcs && this.state.npcs[0].name} style={{ width: 220 }} onChange={this.handleNpcSelectionChange.bind(this)}>
          {
            this.state.npcs && this.state.npcs.length > 0
              ? this.state.npcs.map((npc, idx) => {
                return <Option value={npc.name} key={idx}>{npc.name}</Option>
              })
              : []
          }
        </Select>

        <Select
          mode="multiple"
          allowClear
          style={{width: '100%'}}
          placeholder="Please select"
          // @ts-ignore
          defaultValue={defaultFilters}
          onChange={this.handleFiltersChange.bind(this)}
        >
          {filterSelections}
        </Select>
        {contents}
      </div>
    );
  }

  async populateJourneys() {
    const response = await fetch('journey');
    const data = await response.json();

    let playerEnterLog: string;

    const journeys = data.map((j: { text: string; }) => {
      const entries = j.text
        .split('\n')
        .map(entry => {
          const reTime = new RegExp('^\\[[^A-Za-z]+]');
          const result = reTime.exec(entry)
          const time = result ? result[0].trim() : '[NA]';
          const char = entry.replace(`${time}  `, '').split(':')[0].trim() || '[Unknown]';
          const content = entry.replace(`${time}  `, '').replace(`${char}: `, '').trim();

          if(char === 'System' && content.includes('has entered the realm')) {
            playerEnterLog = content;
          }

          if(playerEnterLog && playerEnterLog.includes(char)) {
            const players = this.state.players || [];
            if(players.indexOf(char) === -1) {
              players.push(char);
              this.setState({players})
              updateFilterSelections(char);
            }
          }

          let type = EntryType.None;
          if(this.isNPC(char, content)) {
            type = EntryType.NpcDescription;
            const npcs = this.state.npcs || [];
            if(!npcs.find(npc => npc.name === char)) {
              npcs.push({name: char, title: content});
              this.setState({npcs})
            }
          } else if(this.isCorpse(content)) {
            type = EntryType.CorpseDescription;
          }

          return {
            time, char, content, type
          } as Entry
        })
      return entries;
    })
    this.setState({data: journeys, loading: false});
  }

  async startBroadcast() {
    // @ts-ignore
    if (this.state.connection?._connectionStarted) {
      try {
          const broadcastMessage = {
            user: 'Derek',
            message: 'This is broadcast message from one client. Send by `IHubContext`',
          };
          // await fetch(`${this.apiUrlBase}/journey/loop-start`, {
          //   method: 'POST',
          //   body: JSON.stringify(broadcastMessage),
          //   headers: {
          //     'Content-Type': 'application/json'
          //   }
          // });

          // Send message to server via signalR
          try {
            await this.state.connection?.send('SendMessage', broadcastMessage);
          }
          catch(e) {
            console.log(e);
          }
      }
      catch(e) {
        console.log('Sending message failed.', e);
      }
    }
    else {
      alert('No connection to server yet.');
    }
  }

  async stopBroadcast() {
    // @ts-ignore
    if (this.state.connection?._connectionStarted) {
      try {
          await fetch(`${this.apiUrlBase}/journey/loop-stop`, {
            method: 'POST',
            body: '',
            headers: {
              'Content-Type': 'application/json'
            }
          });
      } catch(e) {
        console.log('Sending message failed.', e);
      }
    } else {
      alert('No connection to server yet.');
    }
  }
}

