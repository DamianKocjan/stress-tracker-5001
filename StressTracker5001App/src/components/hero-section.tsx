import { Link } from "@tanstack/react-router";
import { ArrowRight, Sparkles } from "lucide-react";
import { Button } from "./ui/button";

export function HeroSection() {
  return (
    <section className="relative py-20 sm:py-28 lg:py-36 overflow-hidden">
      <div className="absolute inset-0 bg-muted/20" />

      {/* Animated background elements */}
      <div className="absolute top-20 left-10 w-72 h-72 bg-foreground/5 rounded-full blur-3xl animate-float" />
      <div className="absolute bottom-20 right-10 w-96 h-96 bg-foreground/5 rounded-full blur-3xl animate-float delay-300" />

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="max-w-4xl mx-auto text-center">
          <div className="inline-flex items-center gap-2 bg-foreground/5 text-foreground px-4 py-2 rounded-full text-sm font-medium mb-6 transition-all duration-500 delay-100 animate-fade-up opacity-100">
            <Sparkles className="h-4 w-4" />
            Visual task management made simple
          </div>
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold text-foreground mb-6 text-balance leading-tight transition-all duration-700 delay-200 animate-fade-up opacity-100">
            Organize Your Work,{" "}
            <span className="relative inline-block">
              Reduce Your Stress
              <svg
                className="absolute -bottom-2 left-0 w-full"
                height="8"
                viewBox="0 0 300 8"
                fill="none"
              >
                <path
                  d="M1 5.5C50 2.5 100 1 150 1.5C200 2 250 3.5 299 5.5"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  className="animate-[dash_1.5s_ease-in-out_forwards]"
                  strokeDasharray="300"
                  strokeDashoffset="300"
                  style={{ animation: "dash 1.5s ease-in-out 0.8s forwards" }}
                />
              </svg>
            </span>
          </h1>
          <p className="text-lg sm:text-xl text-muted-foreground mb-8 text-pretty max-w-2xl mx-auto leading-relaxed transition-all duration-700 delay-300 animate-fade-up opacity-100">
            A powerful Kanban board system designed to help you track tasks,
            manage to-do lists, and monitor progress on all your
            initiatives—visually and effortlessly.
          </p>
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 transition-all duration-700 animate-fade-up opacity-100 delay-400">
            <Button
              size="lg"
              className="w-full sm:w-auto text-base px-8 group hover:scale-105 transition-transform"
              asChild
            >
              <Link to="/register" search={{ redirect: "/" }}>
                Get Started Free
                <ArrowRight className="ml-2 h-4 w-4 group-hover:translate-x-1 transition-transform" />
              </Link>
            </Button>
            <Button
              size="lg"
              variant="outline"
              className="w-full sm:w-auto text-base px-8 hover:scale-105 transition-transform bg-transparent"
            >
              Learn More
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
}
