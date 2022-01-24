import logo from './logo.svg';
import React, { Component } from 'react';
import './App.css';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet'


class App extends Component {

  constructor(props) {
    super(props);
    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  state = {
    coordinates: [],
    value: ''
  }

  componentDidMount() {
    // fetch('https://localhost:7276/WeatherForecast?customerIds=0974623b-4259-40be-8f40-5d39c7eff1ca')
    //   .then(res => res.json())
    //   .then((data) => {
    //     this.setState({ coordinates: data })
    //   })
    //   .catch(console.log)
  }

  handleSubmit(event) {
    alert('A name was submitted: ' + this.state.value);
    event.preventDefault();
    fetch('https://localhost:7276/WeatherForecast?commaSeperatedCustomerIds=' + this.state.value)
      .then(res => res.json())
      .then((data) => {
        this.setState({ coordinates: data })
      })
      .catch(console.log)
  }

  handleChange(event) {
    this.setState({ value: event.target.value });
  }

  render() {

    return (
      <div>
        <form onSubmit={this.handleSubmit}>
          <label>CustomerIds: <input type="text" value={this.state.value} onChange={this.handleChange} /></label>
          <input type="submit" value="Refresh" />
        </form>

        <MapContainer center={[47.0645, 8.4897]} zoom={8} scrollWheelZoom={false}>
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {this.state.coordinates.map((coordinate, index) => (
            <Marker key={index} position={[Number(coordinate.latitude), coordinate.longitude]}>
              <Popup>
                {coordinate.fullAddress}
              </Popup>
            </Marker>
          ))}
        </MapContainer >
      </div>
    );
  }
}

export default App;
