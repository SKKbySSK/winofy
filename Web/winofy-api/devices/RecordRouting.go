package devices

import (
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

	values, err := getBodyParameters(request, []string{ "device_id", "axes", "SI" })

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

	exec := "INSERT INTO Records (DeviceId, Date, Axes, SI) VALUES (?, NOW(), ?, ?)"
	_, err = sqlConnection.Exec(exec, deviceId, axes, si)

	if err != nil {
		response.WriteHeader(400)
		return
	}

	q := "SELECT DeviceToken FROM Notifications WHERE Username = ? AND NotificationType = ?"
	rows, err := sqlConnection.Query(q, *username, results.NotificationFCM)

	if err != nil {
		response.WriteHeader(500)
		log.Fatal(err)
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

	for _, token := range tokens {
		err = fcm.SendNotification("SI Received", "SI : " + strconv.FormatFloat(si, 'f', 3, 32), token)

		if err != nil {
			log.Println(err)
		}
	}

	response.WriteHeader(200)
}
