package internal

import (
	"github.com/emicklei/go-restful"
	"../results"
	"log"
	"strconv"
)

type NotificationRouting struct {

}

func (notif *NotificationRouting) Route(ws *restful.WebService) {
	ws.Route(ws.PUT("internal/notification").To(notif.putFunc))
	ws.Route(ws.POST("internal/notification").To(notif.postFunc))
}

func (notif *NotificationRouting) getResult(success bool) *results.NotificationResult {
	res := new(results.NotificationResult)
	res.Success = success

	return res
}

func (not *NotificationRouting) postFunc(request *restful.Request, response *restful.Response) {
	//TODO Handle the earthquake data, temperature, humidity

	if !isAuthorized(request) {
		response.WriteHeader(401)
		return
	}

	username, err := getUsernameFromToken(request.HeaderParameter("token"))

	if err != nil {
		response.WriteHeader(400)
		return
	}

	values, err := getBodyParameters(request, []string{ "notify" })

	if err != nil {
		response.WriteHeader(400)
		return
	}

	notify, _ := strconv.ParseBool(values[0])

	if !notify {
		response.WriteHeader(200)
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

	fcm, err := CreateFcmNotification()

	if err != nil {
		response.WriteHeader(500)
		log.Println(err)
		return
	}

	for _, token := range tokens {
		err = fcm.SendNotification("This is an notification", "Are you there?", token)

		if err != nil {
			log.Println(err)
		}
	}

	response.WriteHeader(200)
}

func (not *NotificationRouting) putFunc(request *restful.Request, response *restful.Response) {
	if !isAuthorized(request) {
		writeJsonResponse(response, not.getResult(false), 401)
		return
	}

	username, err := getUsernameFromToken(request.HeaderParameter("token"))

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 400)
		return
	}

	values, err := getBodyParameters(request, []string{ "device_token", "type" })
	dev_token := values[0]
	dev_type := values[1]

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 400)
		return
	}

	rows, err := sqlConnection.Query("SELECT DeviceToken, NotificationType FROM Notifications WHERE Username = ?", *username)
	defer rows.Close()

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 500)
		return
	}

	var (
		r_token string
		r_type string
	)

	update := false
	for rows.Next() {
		rows.Scan(&r_token, &r_type)

		if r_token == dev_token && r_type == dev_type {
			writeJsonResponse(response, not.getResult(true), 200)
			return
		}

		if r_token == dev_token {
			update = true
			break
		}
	}

	if update {
		exec := "UPDATE Notifications SET Username = ?, NotificationType = ? WHERE DeviceToken = ?"
		println(exec)
		_, err = sqlConnection.Exec(exec, *username, dev_type, dev_token)

		if err != nil {
			writeJsonResponse(response, not.getResult(false), 500)
			return
		}

		writeJsonResponse(response, not.getResult(true), 200)
		return
	}

	exec := "INSERT INTO Notifications (Username, DeviceToken, NotificationType) VALUES (?, ?, ?)"
	println(exec)
	_, err = sqlConnection.Exec(exec, *username, dev_token, dev_type)

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 500)
		return
	}

	writeJsonResponse(response, not.getResult(true), 200)
}
