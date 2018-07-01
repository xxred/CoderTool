import './css/site.css'
import 'core-js/es6/promise'
import 'core-js/es6/array'
import Vue from 'vue'
import iView from 'iview'
import 'iview/dist/styles/iview.css'

Vue.use(iView)

import { app } from './app'

app.$mount('#app')
