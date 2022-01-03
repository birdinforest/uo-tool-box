import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Counter } from './components/Counter';

import './custom.css'
import {LocalJourney} from "./components/LocalJourney";
import {RuntimeJourney} from "./components/Runtimejourney";

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
        <Route path='/local-journey' component={LocalJourney} />
        <Route path='/runtime-journey' component={RuntimeJourney} />
      </Layout>
    );
  }
}
