package cz.muni.fi.cpmprovviz.provenance.api;

import org.springframework.beans.factory.ObjectProvider;
import org.springframework.boot.info.BuildProperties;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api")
public class InfoController {

    private final ApiInfo info;

    /** @param buildProperties the build information the Maven build writes, if there is any */
    public InfoController(ObjectProvider<BuildProperties> buildProperties) {
        BuildProperties build = buildProperties.getIfAvailable();
        this.info = new ApiInfo(
                build == null ? "cpm-prov-viz-provenance-service" : build.getArtifact(),
                build == null || build.getVersion() == null ? "unknown" : build.getVersion());
    }

    /** The service's name and version, which also tells the frontend that the service is reachable. */
    @GetMapping("/info")
    public ApiInfo info() {
        return info;
    }
}
