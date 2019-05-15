package main

import (
	"./internal"
	"encoding/json"
	"flag"
	"github.com/emicklei/go-restful"
	_ "github.com/go-sql-driver/mysql"
	"github.com/jmoiron/sqlx"
	"io/ioutil"
	"net/http"
	"os"
	"./devices"
)

func main()  {
	connection := parseConnectionFile()

	if connection == nil {
		return
	}

	sqlConnection := connectToDatabase(connection)
	internal.SetSqlConnection(sqlConnection)
	devices.SetSqlConnection(sqlConnection)
	defer sqlConnection.Close()

	internal.SetFcmCredentialsFile("fcm/winofy.json")

	routings := []WinofyRouting{
		new(internal.RegisterRouting),
		new(internal.LoginRouting),
		new(internal.ValidRouting),
		new(internal.NotificationRouting),
		new(devices.ListRouting),
		new(devices.RegisterRouting),
		new(devices.RecordRouting),
	}

	ws := new(restful.WebService)
	for _, r := range routings {
		r.Route(ws)
	}
	restful.Add(ws)

	err := http.ListenAndServe(":8080", nil)

	if sayError("Failed to start the API server", err) {
		return
	}
}

func parseConnectionFile() *Connection {
	jsonPath := flag.String("con", "../winofy-db/db/connection.json", "Path to the database connection file")
	flag.Parse()
	println("Connecting to the database...")

	file, err := os.Open(*jsonPath)
	defer file.Close()

	if sayError("Could not open the connection file", err) {
		return nil
	}

	data, err := ioutil.ReadAll(file)

	if sayError("Failed to read the connection file", err) {
		return nil
	}

	connection := new(Connection)
	err = json.Unmarshal(data, &connection)

	if sayError("Failed to parse the connection file", err) {
		return nil
	}

	return connection
}

func connectToDatabase(connection *Connection) *sqlx.DB {
	conStr := connection.Username + ":" + connection.Password + "@tcp(" + connection.Address + ")/" + connection.Database + "?parseTime=true&loc=Asia%2FTokyo"
	sql, err := sqlx.Connect("mysql", conStr)

	if sayError("Failed to connect to the database", err) {
		return nil
	}

	println("Connected to the database : " + connection.Database)
	return sql
}

func sayError(message string, err error) bool {
	if err != nil {
		println(message)
		println(err.Error())
		return true
	}
	return false
}
