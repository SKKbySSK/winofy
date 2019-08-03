package devices

import "github.com/emicklei/go-restful"

type NotificationRouting struct {

}

func (notif *NotificationRouting) Route(ws *restful.WebService) {
	ws.Route(ws.PUT("devices/notification").To(notif.registerFunc))
}

func (notif *NotificationRouting) registerFunc(request *restful.Request, response *restful.Response) {
	if !isAuthorized(request) {
		response.WriteHeader(401)
		return
	}

	_, err := getUsernameFromToken(request.HeaderParameter("token"))
	if err != nil {
		response.WriteHeader(400)
		return
	}

	
}
