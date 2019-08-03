package devices

import (
	"database/sql"
	"fmt"
	"github.com/emicklei/go-restful"
	"log"
	"strconv"
	"../internal"
	"../results"
)

type RecordRouting struct {
}


func (rec *RecordRouting) Route(ws *restful.WebService) {
	ws.Route(ws.POST("devices/record").To(rec.registerFunc))
}

func (rec *RecordRouting) registerFunc(request *restful.Request, response *restful.Response) {
	if !isAuthorized(request) {
		response.WriteHeader(401)
		return
	}

	username, err := getUsernameFromToken(request.HeaderParameter("token"))

	if err != nil {
		response.WriteHeader(400)
		return
	}

	values, err := getBodyParameters(request, []string{ "device_id", "axes", "SI", "temp", "humidity", "window" })

	if err != nil {
		response.WriteHeader(400)
		return
	}

	deviceId := values[0]
	axes, err := strconv.ParseInt(values[1], 10, 32)

	if err != nil {
		response.WriteHeader(400)
		return
	}

	si, err := strconv.ParseFloat(values[2], 32)

	if err != nil {
		response.WriteHeader(400)
		return
	}

	temp, err := strconv.ParseFloat(values[3], 32)

	if err != nil {
		response.WriteHeader(400)
		return
	}

	humid, err := strconv.ParseFloat(values[4], 32)

	if err != nil {
		response.WriteHeader(400)
		return
	}

	window, err := strconv.ParseInt(values[5], 10, 32)

	if err != nil {
		response.WriteHeader(400)
		return
	}

	exec := "INSERT INTO Records (DeviceId, Date, Axes, SI, Temp, Humidity, Window) VALUES (?, NOW(), ?, ?, ?, ?, ?)"
	_, err = sqlConnection.Exec(exec, deviceId, axes, si, temp, humid, window)

	if err != nil {
		response.WriteHeader(400)
		return
	}

	if window == WindowNone {
		return
	}

	q := "SELECT Name FROM Devices WHERE DeviceId = ?"
	row := sqlConnection.QueryRow(q, deviceId)
	var deviceName string

	switch err := row.Scan(&deviceName); err {
	case sql.ErrNoRows:
		log.Println("Could not find the device " + deviceName)
	default:
		log.Println(err)
		return
	}

	q = "SELECT DeviceToken FROM Notifications WHERE Username = ? AND NotificationType = ?"
	rows, err := sqlConnection.Query(q, *username, results.NotificationFCM)
	defer rows.Close()

	if err != nil {
		response.WriteHeader(500)
		log.Println(err)
		return
	}

	var (
		dev_token string
		tokens []string
	)

	for rows.Next() {
		rows.Scan(&dev_token)
		tokens = append(tokens, dev_token)
	}

	fcm, err := internal.CreateFcmNotification()

	if err != nil {
		response.WriteHeader(500)
		log.Println(err)
		return
	}


	var title string

	switch window {
	case WindowOpen:
		title = fmt.Sprint("%sが窓を開きました", deviceName)
	case WindowClose:
		title = fmt.Sprint("%sが窓を閉じました", deviceName)
	default:
		return
	}

	for _, token := range tokens {

		err = fcm.SendNotification(title, fmt.Sprint("温度:%.1f, 湿度:%.1f, SI値:%.2f", temp, humid, si), token)

		if err != nil {
			log.Println(err)
		}
	}

	response.WriteHeader(200)
}
