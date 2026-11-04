package cz.muni.fi.cpmprovviz.provenance.api;

/**
 * @param name the service's name
 * @param version the service's version, or {@code unknown} when the build left no build information
 */
public record ApiInfo(String name, String version) {}
